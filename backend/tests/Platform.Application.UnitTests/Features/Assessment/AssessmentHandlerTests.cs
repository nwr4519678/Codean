using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Platform.Application.Common.Abstractions;
using Platform.Application.Features.Assessment.Commands;
using Platform.Application.Features.Assessment.Dtos;
using Platform.Domain.Entities;
using Xunit;

namespace Platform.Application.UnitTests.Features.Assessment;

public class AssessmentHandlerTests
{
    private readonly DateTimeOffset _now = new(2026, 8, 7, 12, 0, 0, TimeSpan.Zero);

    // ── CreateExam ────────────────────────────────────────────────────────────

    public class CreateExamHandlerTests
    {
        private readonly IRepository<Exam> _exams = Substitute.For<IRepository<Exam>>();
        private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
        private readonly ICurrentUser _current = Substitute.For<ICurrentUser>();
        private readonly IClock _clock = Substitute.For<IClock>();
        private readonly CreateExamHandler _sut;

        public CreateExamHandlerTests()
        {
            _clock.UtcNow.Returns(new DateTimeOffset(2026, 8, 7, 12, 0, 0, TimeSpan.Zero));
            _current.UserId.Returns(10L);
            _sut = new CreateExamHandler(_exams, _uow, _current, _clock);
        }

        [Fact]
        public async Task Handle_WhenUnauthenticated_ShouldReturnUnauthorized()
        {
            _current.UserId.Returns((long?)null);
            var result = await _sut.Handle(new CreateExamCommand(1L, "Midterm", "Desc", 60, 100, 50, null, null), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("auth.unauthenticated");
        }

        [Fact]
        public async Task Handle_WhenValid_ShouldCreateExam()
        {
            var cmd = new CreateExamCommand(1L, "Midterm Exam", "Algebra", 90, 100, 60, null, null);

            var result = await _sut.Handle(cmd, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value!.Title.Should().Be("Midterm Exam");
            result.Value.DurationMinutes.Should().Be(90);

            await _exams.Received(1).AddAsync(Arg.Is<Exam>(e => e.TeacherId == 10L && e.Title == "Midterm Exam"), Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }

    // ── StartExamAttempt ──────────────────────────────────────────────────────

    public class StartExamAttemptHandlerTests
    {
        private readonly IRepository<Exam> _exams = Substitute.For<IRepository<Exam>>();
        private readonly IRepository<ExamAttempt> _attempts = Substitute.For<IRepository<ExamAttempt>>();
        private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
        private readonly ICurrentUser _current = Substitute.For<ICurrentUser>();
        private readonly IClock _clock = Substitute.For<IClock>();
        private readonly StartExamAttemptHandler _sut;

        public StartExamAttemptHandlerTests()
        {
            _clock.UtcNow.Returns(new DateTimeOffset(2026, 8, 7, 12, 0, 0, TimeSpan.Zero));
            _current.UserId.Returns(50L);
            _sut = new StartExamAttemptHandler(_exams, _attempts, _uow, _current, _clock);
        }

        [Fact]
        public async Task Handle_WhenExamUnpublished_ShouldReturnValidationError()
        {
            _exams.GetByIdAsync(1L, Arg.Any<CancellationToken>()).Returns(new Exam { Id = 1, IsPublished = false });

            var result = await _sut.Handle(new StartExamAttemptCommand(1L), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("exams.not_published");
        }

        [Fact]
        public async Task Handle_WhenExamPublished_ShouldStartAttempt()
        {
            _exams.GetByIdAsync(1L, Arg.Any<CancellationToken>()).Returns(new Exam { Id = 1, IsPublished = true });

            var result = await _sut.Handle(new StartExamAttemptCommand(1L), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value!.StudentId.Should().Be(50L);
            result.Value.Status.Should().Be("InProgress");

            await _attempts.Received(1).AddAsync(Arg.Is<ExamAttempt>(a => a.StudentId == 50L && a.ExamId == 1L), Arg.Any<CancellationToken>());
        }
    }

    // ── GradeHomeworkSubmission ───────────────────────────────────────────────

    public class GradeHomeworkSubmissionHandlerTests
    {
        private readonly IRepository<HomeworkSubmission> _submissions = Substitute.For<IRepository<HomeworkSubmission>>();
        private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
        private readonly ICurrentUser _current = Substitute.For<ICurrentUser>();
        private readonly GradeHomeworkSubmissionHandler _sut;

        public GradeHomeworkSubmissionHandlerTests()
        {
            _current.UserId.Returns(10L);
            _sut = new GradeHomeworkSubmissionHandler(_submissions, _uow, _current);
        }

        [Fact]
        public async Task Handle_WhenSubmissionNotFound_ShouldReturnNotFound()
        {
            _submissions.GetByIdAsync(99L, Arg.Any<CancellationToken>()).Returns((HomeworkSubmission?)null);

            var result = await _sut.Handle(new GradeHomeworkSubmissionCommand(99L, 95, "Great job!"), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("submissions.not_found");
        }

        [Fact]
        public async Task Handle_WhenValid_ShouldSetGradeAndFeedback()
        {
            var sub = new HomeworkSubmission { Id = 1L, StudentId = 50L, Status = "Submitted" };
            _submissions.GetByIdAsync(1L, Arg.Any<CancellationToken>()).Returns(sub);

            var result = await _sut.Handle(new GradeHomeworkSubmissionCommand(1L, 95, "Well written"), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            sub.Grade.Should().Be(95);
            sub.Feedback.Should().Be("Well written");
            sub.Status.Should().Be("Graded");
            _submissions.Received(1).Update(sub);
        }
    }
}
