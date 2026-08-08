using System;
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

// ── CreateCodingChallenge ─────────────────────────────────────────────────

public sealed class CreateCodingChallengeHandler
    : IRequestHandler<CreateCodingChallengeCommand, Result<CodingChallengeResponse>>
{
    private readonly IRepository<CodingChallenge> _challenges;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;

    public CreateCodingChallengeHandler(
        IRepository<CodingChallenge> challenges,
        IUnitOfWork uow,
        ICurrentUser currentUser,
        IClock clock)
    {
        _challenges = challenges;
        _uow = uow;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<CodingChallengeResponse>> Handle(
        CreateCodingChallengeCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<CodingChallengeResponse>.Failure(
                Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        var challenge = new CodingChallenge
        {
            TeacherId  = _currentUser.UserId.Value,
            Title      = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Language   = request.Language.Trim().ToLowerInvariant(),
            StarterCode = request.StarterCode?.Trim(),
            TestCases  = request.TestCases,
            Difficulty = request.Difficulty,
            Marks      = request.Marks,
            CreatedAt  = _clock.UtcNow.UtcDateTime
        };

        await _challenges.AddAsync(challenge, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<CodingChallengeResponse>.Success(challenge.ToResponse());
    }
}

// ── UpdateCodingChallenge ─────────────────────────────────────────────────

public sealed class UpdateCodingChallengeHandler
    : IRequestHandler<UpdateCodingChallengeCommand, Result<CodingChallengeResponse>>
{
    private readonly IRepository<CodingChallenge> _challenges;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public UpdateCodingChallengeHandler(
        IRepository<CodingChallenge> challenges,
        IUnitOfWork uow,
        ICurrentUser currentUser)
    {
        _challenges = challenges;
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<CodingChallengeResponse>> Handle(
        UpdateCodingChallengeCommand request, CancellationToken ct)
    {
        if (!_currentUser.UserId.HasValue)
            return Result<CodingChallengeResponse>.Failure(
                Error.Unauthorized("auth.unauthenticated", "You must be logged in."));

        var challenge = await _challenges.GetByIdAsync(request.ChallengeId, ct);
        if (challenge is null)
            return Result<CodingChallengeResponse>.Failure(
                Error.NotFound("challenges.not_found", $"Challenge {request.ChallengeId} not found."));

        if (challenge.TeacherId != _currentUser.UserId.Value && !_currentUser.IsInRole("Admin"))
            return Result<CodingChallengeResponse>.Failure(
                Error.Forbidden("auth.forbidden", "You do not have permission."));

        challenge.Title       = request.Title.Trim();
        challenge.Description = request.Description?.Trim();
        challenge.StarterCode = request.StarterCode?.Trim();
        challenge.TestCases   = request.TestCases;
        challenge.Difficulty  = request.Difficulty;
        challenge.Marks       = request.Marks;

        _challenges.Update(challenge);
        await _uow.SaveChangesAsync(ct);

        return Result<CodingChallengeResponse>.Success(challenge.ToResponse());
    }
}

// ── GetCodingChallengeById ────────────────────────────────────────────────

public sealed class GetCodingChallengeByIdHandler
    : IRequestHandler<GetCodingChallengeByIdQuery, Result<CodingChallengeResponse>>
{
    private readonly IRepository<CodingChallenge> _challenges;
    public GetCodingChallengeByIdHandler(IRepository<CodingChallenge> challenges)
        => _challenges = challenges;

    public async Task<Result<CodingChallengeResponse>> Handle(
        GetCodingChallengeByIdQuery request, CancellationToken ct)
    {
        var challenge = await _challenges.GetByIdAsync(request.ChallengeId, ct);
        if (challenge is null)
            return Result<CodingChallengeResponse>.Failure(
                Error.NotFound("challenges.not_found", $"Challenge {request.ChallengeId} not found."));

        return Result<CodingChallengeResponse>.Success(challenge.ToResponse());
    }
}

public sealed class GetCodingChallengesHandler
    : IRequestHandler<GetCodingChallengesQuery, Result<IReadOnlyList<CodingChallengeResponse>>>
{
    private readonly IRepository<CodingChallenge> _challenges;
    public GetCodingChallengesHandler(IRepository<CodingChallenge> challenges) => _challenges = challenges;

    public async Task<Result<IReadOnlyList<CodingChallengeResponse>>> Handle(
        GetCodingChallengesQuery request, CancellationToken ct)
    {
        var challenges = await _challenges.ListAsync(ct: ct);
        var responses = challenges
            .OrderByDescending(challenge => challenge.CreatedAt)
            .Select(challenge => challenge.ToResponse())
            .ToList();
        return Result<IReadOnlyList<CodingChallengeResponse>>.Success(responses);
    }
}
