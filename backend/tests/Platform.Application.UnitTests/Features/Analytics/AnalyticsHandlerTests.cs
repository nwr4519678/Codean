using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Platform.Application.Common.Abstractions;
using Platform.Application.Features.Analytics.Dtos;
using Platform.Application.Features.Analytics.Queries;
using Platform.Domain.Entities;
using Xunit;

namespace Platform.Application.UnitTests.Features.Analytics;

public class AnalyticsHandlerTests
{
    private readonly DateTimeOffset _now = new(2026, 8, 7, 12, 0, 0, TimeSpan.Zero);

    public class GetPlatformOverviewHandlerTests
    {
        private readonly IRepository<User> _users = Substitute.For<IRepository<User>>();
        private readonly IRepository<Role> _roles = Substitute.For<IRepository<Role>>();
        private readonly IRepository<Course> _courses = Substitute.For<IRepository<Course>>();
        private readonly IRepository<CodingSubmission> _submissions = Substitute.For<IRepository<CodingSubmission>>();
        private readonly IRepository<Payment> _payments = Substitute.For<IRepository<Payment>>();
        private readonly IClock _clock = Substitute.For<IClock>();
        private readonly GetPlatformOverviewHandler _sut;

        public GetPlatformOverviewHandlerTests()
        {
            _clock.UtcNow.Returns(new DateTimeOffset(2026, 8, 7, 12, 0, 0, TimeSpan.Zero));

            // Stub roles so dynamic role ID resolution works
            var stubRoles = new List<Role>
            {
                new Role { Id = 1, Name = "Student" },
                new Role { Id = 2, Name = "Teacher" },
                new Role { Id = 3, Name = "Admin" },
            }.AsQueryable();
            _roles.Query().Returns(stubRoles);

            // Stub users — Query() returns empty (monthly enrollment grouping)
            _users.Query().Returns(new List<User>().AsQueryable());

            _sut = new GetPlatformOverviewHandler(_users, _roles, _courses, _submissions, _payments, _clock);
        }

        [Fact]
        public async Task Handle_ShouldReturnAggregatedMetrics()
        {
            // CountAsync is called multiple times: totalStudents, totalTeachers, totalUsers,
            // activeCourses (via _courses), totalSubmissions, newStudentsThisMonth,
            // newTeachersThisMonth, passedSubmissions
            _users.CountAsync(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
                  .Returns(80, 20, 100, 5, 2, 450);   // students, teachers, total, newStudents, newTeachers, passed
            _courses.CountAsync(Arg.Any<Expression<Func<Course, bool>>>(), Arg.Any<CancellationToken>()).Returns(15);
            _submissions.CountAsync(Arg.Any<Expression<Func<CodingSubmission, bool>>>(), Arg.Any<CancellationToken>()).Returns(500, 450);

            _payments.ListAsync(Arg.Any<Expression<Func<Payment, bool>>>(), Arg.Any<CancellationToken>())
                .Returns(new List<Payment>
                {
                    new Payment { Amount = 500, Status = "Paid" },
                    new Payment { Amount = 300, Status = "Paid" }
                });

            var result = await _sut.Handle(new GetPlatformOverviewQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value!.TotalStudents.Should().Be(80);
            result.Value.TotalTeachers.Should().Be(20);
            result.Value.ActiveCourses.Should().Be(15);
            result.Value.TotalSubmissions.Should().Be(500);
            result.Value.TotalRevenue.Should().Be(800);
            result.Value.MonthlyEnrollments.Should().NotBeNull();
        }
    }
}
