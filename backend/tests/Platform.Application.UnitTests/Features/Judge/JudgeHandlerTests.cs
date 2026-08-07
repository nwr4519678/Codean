using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Platform.Application.Common.Abstractions;
using Platform.Application.Common.Contracts.Judge;
using Platform.Application.Features.Judge.Commands;
using Platform.Application.Features.Judge.Dtos;
using Platform.Domain.Entities;
using Xunit;

namespace Platform.Application.UnitTests.Features.Judge;

public class JudgeHandlerTests
{
    private readonly DateTimeOffset _now = new(2026, 8, 7, 12, 0, 0, TimeSpan.Zero);

    // ── CreateCodingChallenge ─────────────────────────────────────────────────

    public class CreateCodingChallengeHandlerTests
    {
        private readonly IRepository<CodingChallenge> _challenges = Substitute.For<IRepository<CodingChallenge>>();
        private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
        private readonly ICurrentUser _current = Substitute.For<ICurrentUser>();
        private readonly IClock _clock = Substitute.For<IClock>();
        private readonly CreateCodingChallengeHandler _sut;

        public CreateCodingChallengeHandlerTests()
        {
            _clock.UtcNow.Returns(new DateTimeOffset(2026, 8, 7, 12, 0, 0, TimeSpan.Zero));
            _current.UserId.Returns(10L);
            _sut = new CreateCodingChallengeHandler(_challenges, _uow, _current, _clock);
        }

        [Fact]
        public async Task Handle_WhenUnauthenticated_ShouldReturnUnauthorized()
        {
            _current.UserId.Returns((long?)null);
            var result = await _sut.Handle(
                new CreateCodingChallengeCommand("Title", "Desc", "python", "print(1)", "[]", "Easy", 10),
                CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("auth.unauthenticated");
        }

        [Fact]
        public async Task Handle_WhenValid_ShouldCreateChallenge()
        {
            var testCases = JsonSerializer.Serialize(new[] { new { Input = "1", ExpectedOutput = "1" } });
            var cmd = new CreateCodingChallengeCommand("Two Sum", "Desc", "python3", "def solve(): pass", testCases, "Medium", 50);

            var result = await _sut.Handle(cmd, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value!.Title.Should().Be("Two Sum");
            result.Value.Language.Should().Be("python3");
            result.Value.Difficulty.Should().Be("Medium");

            await _challenges.Received(1).AddAsync(
                Arg.Is<CodingChallenge>(c => c.TeacherId == 10L && c.Marks == 50),
                Arg.Any<CancellationToken>());
        }
    }

    // ── SubmitCodeChallenge (Async Enqueue) ───────────────────────────────────

    public class SubmitCodeChallengeHandlerTests
    {
        private readonly IRepository<CodingChallenge> _challenges = Substitute.For<IRepository<CodingChallenge>>();
        private readonly IRepository<CodingSubmission> _submissions = Substitute.For<IRepository<CodingSubmission>>();
        private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
        private readonly IJudgeService _judgeService = Substitute.For<IJudgeService>();
        private readonly ICurrentUser _current = Substitute.For<ICurrentUser>();
        private readonly IClock _clock = Substitute.For<IClock>();
        private readonly SubmitCodeChallengeHandler _sut;

        public SubmitCodeChallengeHandlerTests()
        {
            _clock.UtcNow.Returns(new DateTimeOffset(2026, 8, 7, 12, 0, 0, TimeSpan.Zero));
            _current.UserId.Returns(50L);
            _sut = new SubmitCodeChallengeHandler(_challenges, _submissions, _uow, _judgeService, _current, _clock);
        }

        [Fact]
        public async Task Handle_WhenChallengeNotFound_ShouldReturnNotFound()
        {
            _challenges.GetByIdAsync(99L, Arg.Any<CancellationToken>()).Returns((CodingChallenge?)null);

            var result = await _sut.Handle(
                new SubmitCodeChallengeCommand(99L, "print('hello')", "python3"),
                CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("challenges.not_found");
        }

        [Fact]
        public async Task Handle_WhenValid_ShouldEnqueueAndReturnQueuedStatus()
        {
            var testCases = JsonSerializer.Serialize(new[] { new { Input = "1", ExpectedOutput = "1" } });
            _challenges.GetByIdAsync(1L, Arg.Any<CancellationToken>())
                .Returns(new CodingChallenge { Id = 1, TeacherId = 10L, Language = "python3", TestCases = testCases });

            _judgeService
                .SubmitAsync(Arg.Any<CodeExecutionRequest>(), Arg.Any<CancellationToken>())
                .Returns(new ExecutionSubmissionResponse(
                    "exec_abc123", 0L, ExecutionStatus.Queued,
                    new DateTime(2026, 8, 7, 12, 0, 0, DateTimeKind.Utc)));

            var result = await _sut.Handle(
                new SubmitCodeChallengeCommand(1L, "print('hello')", "python3"),
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value!.Status.Should().Be("Queued");
            result.Value.ExecutionId.Should().Be("exec_abc123");

            // Should call judge with correct request
            await _judgeService.Received(1).SubmitAsync(
                Arg.Is<CodeExecutionRequest>(r =>
                    r.Language == "python3" &&
                    r.SourceCode == "print('hello')"),
                Arg.Any<CancellationToken>());

            // Should save twice — once for initial row, once for executionId update
            await _uow.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_WhenUnauthenticated_ShouldReturnUnauthorized()
        {
            _current.UserId.Returns((long?)null);

            var result = await _sut.Handle(
                new SubmitCodeChallengeCommand(1L, "print('hello')", "python3"),
                CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("auth.unauthenticated");
            await _judgeService.DidNotReceive().SubmitAsync(Arg.Any<CodeExecutionRequest>(), Arg.Any<CancellationToken>());
        }
    }

    // ── GetSubmissionStatus (Polling + Score Reconciliation) ─────────────────

    public class GetSubmissionStatusHandlerTests
    {
        private readonly IRepository<CodingSubmission> _submissions = Substitute.For<IRepository<CodingSubmission>>();
        private readonly IJudgeService _judgeService = Substitute.For<IJudgeService>();
        private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
        private readonly GetSubmissionStatusHandler _sut;

        public GetSubmissionStatusHandlerTests()
            => _sut = new GetSubmissionStatusHandler(_submissions, _judgeService, _uow);

        [Fact]
        public async Task Handle_WhenSubmissionNotFound_ShouldReturnNotFound()
        {
            _submissions.GetByIdAsync(99L, Arg.Any<CancellationToken>()).Returns((CodingSubmission?)null);

            var result = await _sut.Handle(new GetSubmissionStatusQuery(99L), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("submissions.not_found");
        }

        [Fact]
        public async Task Handle_WhenJudgeCompleted_ShouldReconcileScore()
        {
            var submission = new CodingSubmission
            {
                Id = 1L,
                CodingChallengeId = 5L,
                StudentId = 50L,
                Status = "Queued",
                ExecutionResult = "exec_abc123"
            };
            _submissions.GetByIdAsync(1L, Arg.Any<CancellationToken>()).Returns(submission);

            _judgeService
                .GetResultAsync("exec_abc123", Arg.Any<CancellationToken>())
                .Returns(new CodeExecutionResult(
                    "exec_abc123", 1L, ExecutionStatus.Completed,
                    Verdict.Accepted, InfrastructureFailure.None,
                    PassedTestCases: 5, TotalTestCases: 5,
                    TestCaseResults: [],
                    CompilationOutput: null,
                    StandardOutput: null,
                    StandardError: null,
                    ExitCode: 0,
                    ExecutionTimeMs: 125.5,
                    MemoryUsedKb: 4096,
                    FailureReason: null));

            var result = await _sut.Handle(new GetSubmissionStatusQuery(1L), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value!.Verdict.Should().Be("Accepted");
            result.Value.PassedTestCases.Should().Be(5);

            // Score should be reconciled to 100%
            submission.Score.Should().Be(100);
            submission.Status.Should().Be("Completed");
            _submissions.Received(1).Update(submission);
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_WhenJudgeStillRunning_ShouldReturnQueuedWithoutUpdatingScore()
        {
            var submission = new CodingSubmission
            {
                Id = 2L,
                Status = "Queued",
                ExecutionResult = "exec_running"
            };
            _submissions.GetByIdAsync(2L, Arg.Any<CancellationToken>()).Returns(submission);

            _judgeService
                .GetResultAsync("exec_running", Arg.Any<CancellationToken>())
                .Returns(new CodeExecutionResult(
                    "exec_running", 2L, ExecutionStatus.Running,
                    null, InfrastructureFailure.None,
                    0, 0, [], null, null, null, null, 0, 0, null));

            var result = await _sut.Handle(new GetSubmissionStatusQuery(2L), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            submission.Score.Should().BeNull(); // not reconciled yet
            _submissions.DidNotReceive().Update(Arg.Any<CodingSubmission>());
        }
    }
}
