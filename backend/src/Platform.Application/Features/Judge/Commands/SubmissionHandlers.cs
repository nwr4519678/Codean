using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Platform.Application.Common.Abstractions;
using Platform.Application.Common.Contracts.Judge;
using Platform.Application.Features.Judge.Dtos;
using Platform.Application.Features.Judge.Mapping;
using Platform.Domain.Entities;
using Platform.Domain.Results;

namespace Platform.Application.Features.Judge.Commands;

/// <summary>
/// Enqueues a code submission to the Judge Engine asynchronously.
///
/// Flow:
///   1. Validate challenge exists.
///   2. Create a CodingSubmission row with Status = "Queued".
///   3. Call IJudgeService.SubmitAsync — this is a fire-and-forget to the judge worker.
///   4. Store the returned ExecutionId on the submission row.
///   5. Return immediately with SubmissionId + ExecutionId so the client can poll.
///
/// The judge worker (Platform.Judge.Worker) will eventually call back via
/// GetResultAsync, and a Hangfire job (ProcessJudgeResultsJob) will reconcile
/// final scores in the database.
/// </summary>
public sealed class SubmitCodeChallengeHandler
    : IRequestHandler<SubmitCodeChallengeCommand, Result<SubmitCodeResponse>>
{
    private readonly IRepository<CodingChallenge> _challenges;
    private readonly IRepository<CodingSubmission> _submissions;
    private readonly IUnitOfWork _uow;
    private readonly IJudgeService _judgeService;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public SubmitCodeChallengeHandler(
        IRepository<CodingChallenge> challenges,
        IRepository<CodingSubmission> submissions,
        IUnitOfWork uow,
        IJudgeService judgeService,
        ICurrentUser currentUser,
        IClock clock)
    {
        _challenges   = challenges;
        _submissions  = submissions;
        _uow          = uow;
        _judgeService = judgeService;
        _currentUser  = currentUser;
        _clock        = clock;
    }

    public async Task<Result<SubmitCodeResponse>> Handle(
        SubmitCodeChallengeCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<SubmitCodeResponse>.Failure(
                Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        long studentId = _currentUser.UserId.Value;
        var now = _clock.UtcNow.UtcDateTime;

        // 1. Verify challenge exists
        var challenge = await _challenges.GetByIdAsync(request.ChallengeId, ct);
        if (challenge is null)
            return Result<SubmitCodeResponse>.Failure(
                Error.NotFound("challenges.not_found", $"Challenge {request.ChallengeId} not found."));

        // 2. Create a pending submission row — idempotency anchor
        var submission = new CodingSubmission
        {
            CodingChallengeId = request.ChallengeId,
            StudentId         = studentId,
            SourceCode        = request.SourceCode,
            SubmittedAt       = now,
            Status            = "Queued",
            Score             = null,
            ExecutionResult   = null   // will hold executionId after enqueue
        };

        await _submissions.AddAsync(submission, ct);
        await _uow.SaveChangesAsync(ct);  // get the DB-generated SubmissionId

        // 3. Deserialize test cases stored as JSON on the challenge
        var testCaseDtos = ParseTestCases(challenge.TestCases);

        // 4. Build idempotent execution ID using SubmissionId as seed
        var executionId = $"sub_{submission.Id}_{Guid.NewGuid():N}";

        var judgeRequest = new CodeExecutionRequest(
            ExecutionId:   executionId,
            SubmissionId:  submission.Id,
            Language:      request.Language.Trim().ToLowerInvariant(),
            SourceCode:    request.SourceCode,
            TestCases:     testCaseDtos
        );

        // 5. Fire-and-forget — judge worker picks this up from the queue asynchronously
        var judgeResponse = await _judgeService.SubmitAsync(judgeRequest, ct);

        // 6. Persist executionId so GetSubmissionStatus can look it up later
        submission.ExecutionResult = judgeResponse.ExecutionId;
        submission.Status = "Queued";

        _submissions.Update(submission);
        await _uow.SaveChangesAsync(ct);

        return Result<SubmitCodeResponse>.Success(new SubmitCodeResponse(
            submission.Id,
            judgeResponse.ExecutionId,
            "Queued",
            now
        ));
    }

    private static List<TestCaseDto> ParseTestCases(string? testCasesJson)
    {
        if (string.IsNullOrWhiteSpace(testCasesJson))
            return [];

        try
        {
            return JsonSerializer.Deserialize<List<TestCaseDto>>(testCasesJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
        }
        catch
        {
            return [];
        }
    }
}

/// <summary>
/// Polls the judge service for the result of a previously submitted execution.
/// Updates the submission row with final score/verdict once complete.
/// </summary>
public sealed class GetSubmissionStatusHandler
    : IRequestHandler<GetSubmissionStatusQuery, Result<CodeSubmissionResultResponse>>
{
    private readonly IRepository<CodingSubmission> _submissions;
    private readonly IJudgeService _judgeService;
    private readonly IUnitOfWork _uow;

    public GetSubmissionStatusHandler(
        IRepository<CodingSubmission> submissions,
        IJudgeService judgeService,
        IUnitOfWork uow)
    {
        _submissions  = submissions;
        _judgeService = judgeService;
        _uow          = uow;
    }

    public async Task<Result<CodeSubmissionResultResponse>> Handle(
        GetSubmissionStatusQuery request, CancellationToken ct)
    {
        var submission = await _submissions.GetByIdAsync(request.SubmissionId, ct);
        if (submission is null)
            return Result<CodeSubmissionResultResponse>.Failure(
                Error.NotFound("submissions.not_found", $"Submission {request.SubmissionId} not found."));

        var executionId = submission.ExecutionResult;
        if (string.IsNullOrWhiteSpace(executionId))
            return Result<CodeSubmissionResultResponse>.Success(submission.ToResultResponse());

        // Try to get result from judge
        var executionResult = await _judgeService.GetResultAsync(executionId, ct);

        // If judge has finished, reconcile final score
        if (executionResult is not null &&
            executionResult.Status is ExecutionStatus.Completed or ExecutionStatus.Failed)
        {
            var verdict = executionResult.Verdict?.ToString() ?? "Unknown";
            var passed  = executionResult.PassedTestCases;
            var total   = executionResult.TotalTestCases;

            submission.Status = executionResult.Status.ToString();
            submission.Score  = total > 0
                ? Math.Round((decimal)passed / total * 100, 2)
                : 0;

            _submissions.Update(submission);
            await _uow.SaveChangesAsync(ct);
        }

        return Result<CodeSubmissionResultResponse>.Success(
            submission.ToResultResponse(executionResult));
    }
}

/// <summary>
/// Lists all submissions made by the current student for a given challenge.
/// </summary>
public sealed class GetMySubmissionsHandler
    : IRequestHandler<GetMySubmissionsQuery, Result<IReadOnlyList<CodeSubmissionResultResponse>>>
{
    private readonly IRepository<CodingSubmission> _submissions;
    private readonly ICurrentUser _currentUser;

    public GetMySubmissionsHandler(
        IRepository<CodingSubmission> submissions,
        ICurrentUser currentUser)
    {
        _submissions = submissions;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<CodeSubmissionResultResponse>>> Handle(
        GetMySubmissionsQuery request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<IReadOnlyList<CodeSubmissionResultResponse>>.Failure(
                Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        long studentId = _currentUser.UserId.Value;

        var list = await _submissions.ListAsync(
            s => s.CodingChallengeId == request.ChallengeId && s.StudentId == studentId,
            ct);

        var dtos = list
            .OrderByDescending(s => s.SubmittedAt)
            .Select(s => s.ToResultResponse())
            .ToList();

        return Result<IReadOnlyList<CodeSubmissionResultResponse>>.Success(dtos);
    }
}
