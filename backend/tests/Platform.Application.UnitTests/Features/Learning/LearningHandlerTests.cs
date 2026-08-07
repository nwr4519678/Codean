using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Platform.Application.Common.Abstractions;
using Platform.Application.Features.Learning.Commands;
using Platform.Application.Features.Learning.Dtos;
using Platform.Domain.Entities;
using Xunit;

namespace Platform.Application.UnitTests.Features.Learning;

public class LearningHandlerTests
{
    private readonly DateTimeOffset _now = new(2026, 8, 7, 12, 0, 0, TimeSpan.Zero);

    // ── CreateCourse ──────────────────────────────────────────────────────────

    public class CreateCourseHandlerTests
    {
        private readonly IRepository<Course> _courses = Substitute.For<IRepository<Course>>();
        private readonly IRepository<TeacherProfile> _teachers = Substitute.For<IRepository<TeacherProfile>>();
        private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
        private readonly ICurrentUser _current = Substitute.For<ICurrentUser>();
        private readonly IClock _clock = Substitute.For<IClock>();
        private readonly ICacheService _cache = Substitute.For<ICacheService>();
        private readonly CreateCourseHandler _sut;

        public CreateCourseHandlerTests()
        {
            _clock.UtcNow.Returns(new DateTimeOffset(2026, 8, 7, 12, 0, 0, TimeSpan.Zero));
            _current.UserId.Returns(10L);
            _sut = new CreateCourseHandler(_courses, _teachers, _uow, _current, _clock, _cache);
        }

        [Fact]
        public async Task Handle_WhenUnauthenticated_ShouldReturnUnauthorized()
        {
            _current.UserId.Returns((long?)null);
            var cmd = new CreateCourseCommand("C# Basics", "Learn C#", null, "Programming", 100);

            var result = await _sut.Handle(cmd, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("auth.unauthenticated");
        }

        [Fact]
        public async Task Handle_WhenValid_ShouldCreateUnpublishedCourse()
        {
            var cmd = new CreateCourseCommand("C# Basics", "Learn C#", "thumb.jpg", "Programming", 100);

            var result = await _sut.Handle(cmd, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value!.Title.Should().Be("C# Basics");
            result.Value.TeacherId.Should().Be(10L);
            result.Value.IsPublished.Should().BeFalse();

            await _courses.Received(1).AddAsync(Arg.Is<Course>(c =>
                c.Title == "C# Basics" &&
                c.TeacherId == 10L &&
                c.Price == 100
            ), Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }

    // ── UpdateCourse ──────────────────────────────────────────────────────────

    public class UpdateCourseHandlerTests
    {
        private readonly IRepository<Course> _courses = Substitute.For<IRepository<Course>>();
        private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
        private readonly ICurrentUser _current = Substitute.For<ICurrentUser>();
        private readonly IClock _clock = Substitute.For<IClock>();
        private readonly ICacheService _cache = Substitute.For<ICacheService>();
        private readonly UpdateCourseHandler _sut;

        public UpdateCourseHandlerTests()
        {
            _clock.UtcNow.Returns(new DateTimeOffset(2026, 8, 7, 12, 0, 0, TimeSpan.Zero));
            _current.UserId.Returns(10L);
            _sut = new UpdateCourseHandler(_courses, _uow, _current, _clock, _cache);
        }

        [Fact]
        public async Task Handle_WhenNotFound_ShouldReturnNotFound()
        {
            _courses.GetByIdAsync(99L, Arg.Any<CancellationToken>()).Returns((Course?)null);
            var cmd = new UpdateCourseCommand(99L, "Title", "Desc", null, "Cat", 50);

            var result = await _sut.Handle(cmd, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("courses.not_found");
        }

        [Fact]
        public async Task Handle_WhenNotOwner_ShouldReturnForbidden()
        {
            _courses.GetByIdAsync(1L, Arg.Any<CancellationToken>()).Returns(new Course { Id = 1, TeacherId = 999 });
            _current.IsInRole("Admin").Returns(false);

            var result = await _sut.Handle(new UpdateCourseCommand(1L, "T", "D", null, "C", 10), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("auth.forbidden");
        }

        [Fact]
        public async Task Handle_WhenOwner_ShouldUpdateCourse()
        {
            var course = new Course { Id = 1, TeacherId = 10L, Title = "Old Title" };
            _courses.GetByIdAsync(1L, Arg.Any<CancellationToken>()).Returns(course);

            var result = await _sut.Handle(new UpdateCourseCommand(1L, "New Title", "Desc", "thumb", "Cat", 150), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            course.Title.Should().Be("New Title");
            course.Price.Should().Be(150);
            _courses.Received(1).Update(course);
        }
    }

    // ── CreateCourseModule ───────────────────────────────────────────────────

    public class CreateCourseModuleHandlerTests
    {
        private readonly IRepository<Course> _courses = Substitute.For<IRepository<Course>>();
        private readonly IRepository<CourseModule> _modules = Substitute.For<IRepository<CourseModule>>();
        private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
        private readonly ICurrentUser _current = Substitute.For<ICurrentUser>();
        private readonly IClock _clock = Substitute.For<IClock>();
        private readonly ICacheService _cache = Substitute.For<ICacheService>();
        private readonly CreateCourseModuleHandler _sut;

        public CreateCourseModuleHandlerTests()
        {
            _current.UserId.Returns(10L);
            _sut = new CreateCourseModuleHandler(_courses, _modules, _uow, _current, _clock, _cache);
        }

        [Fact]
        public async Task Handle_WhenCourseNotFound_ShouldReturnNotFound()
        {
            _courses.GetByIdAsync(99L, Arg.Any<CancellationToken>()).Returns((Course?)null);
            var result = await _sut.Handle(new CreateCourseModuleCommand(99L, "Module 1", 1, 1, "Desc"), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("courses.not_found");
        }

        [Fact]
        public async Task Handle_WhenValid_ShouldAddModule()
        {
            _courses.GetByIdAsync(1L, Arg.Any<CancellationToken>()).Returns(new Course { Id = 1, TeacherId = 10L });

            var result = await _sut.Handle(new CreateCourseModuleCommand(1L, "Month 1 Basics", 1, 1, "Intro"), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value!.Title.Should().Be("Month 1 Basics");
            await _modules.Received(1).AddAsync(Arg.Is<CourseModule>(m => m.CourseId == 1 && m.MonthNumber == 1), Arg.Any<CancellationToken>());
        }
    }

    // ── TrackLessonProgress ──────────────────────────────────────────────────

    public class TrackLessonProgressHandlerTests
    {
        private readonly IRepository<Lesson> _lessons = Substitute.For<IRepository<Lesson>>();
        private readonly IRepository<StudentProgress> _progresses = Substitute.For<IRepository<StudentProgress>>();
        private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
        private readonly ICurrentUser _current = Substitute.For<ICurrentUser>();
        private readonly IClock _clock = Substitute.For<IClock>();
        private readonly TrackLessonProgressHandler _sut;

        public TrackLessonProgressHandlerTests()
        {
            _current.UserId.Returns(50L);
            _sut = new TrackLessonProgressHandler(_lessons, _progresses, _uow, _current, _clock);
        }

        [Fact]
        public async Task Handle_WhenLessonNotFound_ShouldReturnNotFound()
        {
            _lessons.GetByIdAsync(99L, Arg.Any<CancellationToken>()).Returns((Lesson?)null);
            var result = await _sut.Handle(new TrackLessonProgressCommand(99L, 50, 120), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Error.Code.Should().Be("lessons.not_found");
        }

        [Fact]
        public async Task Handle_WhenNewProgress_ShouldInsertRecord()
        {
            _lessons.GetByIdAsync(5L, Arg.Any<CancellationToken>()).Returns(new Lesson { Id = 5 });
            _progresses.FirstOrDefaultAsync(Arg.Any<System.Linq.Expressions.Expression<Func<StudentProgress, bool>>>(), Arg.Any<CancellationToken>())
                       .Returns((StudentProgress?)null);

            var result = await _sut.Handle(new TrackLessonProgressCommand(5L, 75, 300), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value!.Completion.Should().Be(75);
            await _progresses.Received(1).AddAsync(Arg.Is<StudentProgress>(p => p.StudentId == 50L && p.Completion == 75), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_WhenExistingProgress_ShouldAccumulateWatchTime()
        {
            _lessons.GetByIdAsync(5L, Arg.Any<CancellationToken>()).Returns(new Lesson { Id = 5 });
            var existing = new StudentProgress { StudentId = 50L, LessonId = 5L, Completion = 50, WatchTime = 200 };
            _progresses.FirstOrDefaultAsync(Arg.Any<System.Linq.Expressions.Expression<Func<StudentProgress, bool>>>(), Arg.Any<CancellationToken>())
                       .Returns(existing);

            var result = await _sut.Handle(new TrackLessonProgressCommand(5L, 80, 100), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            existing.Completion.Should().Be(80);
            existing.WatchTime.Should().Be(300);
            _progresses.Received(1).Update(existing);
        }
    }
}
