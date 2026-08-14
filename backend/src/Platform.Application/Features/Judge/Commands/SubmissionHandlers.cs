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
/// Executes a code submission through the configured external compiler API.
///
/// Flow:
///   1. Validate challenge exists.
///   2. Create a submission row.
///   3. Send the source to the external provider.
///   4. Store the provider result and return it immediately.
/// </summary>
public sealed class SubmitCodeChallengeHandler
    : IRequestHandler<SubmitCodeChallengeCommand, Result<SubmitCodeResponse>>
{
    private readonly IRepository<CodingChallenge> _challenges;
    private readonly IRepository<CodingSubmission> _submissions;
    private readonly IUnitOfWork _uow;
    private readonly IJudgeService _codeExecutionService;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public SubmitCodeChallengeHandler(
        IRepository<CodingChallenge> challenges,
        IRepository<CodingSubmission> submissions,
        IUnitOfWork uow,
        IJudgeService codeExecutionService,
        ICurrentUser currentUser,
        IClock clock)
    {
        _challenges   = challenges;
        _submissions  = submissions;
        _uow          = uow;
        _codeExecutionService = codeExecutionService;
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
            Status            = "Running",
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

        // 5. Execute synchronously through the external provider. The provider
        // owns sandboxing, compilation, runtime limits, and isolation.
        var executionResult = await _codeExecutionService.ExecuteAsync(judgeRequest, ct);
        var serializedResult = JsonSerializer.Serialize(executionResult);
        submission.ExecutionResult = serializedResult;
        submission.Status = executionResult.Status.ToString();
        submission.Score = executionResult.TotalTestCases > 0
            ? Math.Round((decimal)executionResult.PassedTestCases / executionResult.TotalTestCases * 100, 2)
            : executionResult.Verdict == Verdict.Accepted ? 100 : 0;

        _submissions.Update(submission);
        await _uow.SaveChangesAsync(ct);

        return Result<SubmitCodeResponse>.Success(new SubmitCodeResponse(
            submission.Id,
            executionResult.ExecutionId,
            executionResult.Status.ToString(),
            now
        ));
    }

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private static List<TestCaseDto> ParseTestCases(string? testCasesJson)
    {
        if (string.IsNullOrWhiteSpace(testCasesJson))
            return [];

        try
        {
            return JsonSerializer.Deserialize<List<TestCaseDto>>(testCasesJson, JsonOptions) ?? [];
        }
        catch
        {
            return [];
        }
    }
}

/// <summary>
/// Returns the provider result already persisted with the submission.
/// </summary>
public sealed class GetSubmissionStatusHandler
    : IRequestHandler<GetSubmissionStatusQuery, Result<CodeSubmissionResultResponse>>
{
    private readonly IRepository<CodingSubmission> _submissions;

    public GetSubmissionStatusHandler(
        IRepository<CodingSubmission> submissions,
        IJudgeService codeExecutionService,
        IUnitOfWork uow)
    {
        _submissions  = submissions;
    }

    public async Task<Result<CodeSubmissionResultResponse>> Handle(
        GetSubmissionStatusQuery request, CancellationToken ct)
    {
        var submission = await _submissions.GetByIdAsync(request.SubmissionId, ct);
        if (submission is null)
            return Result<CodeSubmissionResultResponse>.Failure(
                Error.NotFound("submissions.not_found", $"Submission {request.SubmissionId} not found."));

        if (string.IsNullOrWhiteSpace(submission.ExecutionResult))
            return Result<CodeSubmissionResultResponse>.Success(submission.ToResultResponse());

        CodeExecutionResult? executionResult = null;
        try
        {
            executionResult = JsonSerializer.Deserialize<CodeExecutionResult>(submission.ExecutionResult);
        }
        catch (JsonException) { }

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
