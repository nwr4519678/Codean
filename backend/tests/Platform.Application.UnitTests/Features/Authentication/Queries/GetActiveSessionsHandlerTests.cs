using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Platform.Application.Common.Abstractions;
using Platform.Application.Features.Authentication.Dtos;
using Platform.Application.Features.Authentication.Queries.GetActiveSessions;
using Platform.Domain.Entities;
using Xunit;

namespace Platform.Application.UnitTests.Features.Authentication.Queries;

public class GetActiveSessionsHandlerTests
{
    private readonly IRepository<UserSession> _sessions = Substitute.For<IRepository<UserSession>>();
    private readonly ICurrentUser _current = Substitute.For<ICurrentUser>();
    private readonly GetActiveSessionsHandler _sut;

    public GetActiveSessionsHandlerTests()
    {
        _current.IpAddress.Returns("192.168.1.100");
        _sut = new GetActiveSessionsHandler(_sessions, _current);
    }

    [Fact]
    public async Task Handle_ShouldReturnMappedSessionResponsesWithIsCurrentFlag()
    {
        // Arrange
        var sessionsList = new List<UserSession>
        {
            new() { Id = 1, UserId = 10, IpAddress = "192.168.1.100", DeviceInfo = "Chrome", IsActive = true, LoginAt = DateTime.UtcNow },
            new() { Id = 2, UserId = 10, IpAddress = "10.0.0.1", DeviceInfo = "Firefox", IsActive = true, LoginAt = DateTime.UtcNow },
            new() { Id = 3, UserId = 10, IpAddress = "192.168.1.100", DeviceInfo = "Mobile", IsActive = false, LoginAt = DateTime.UtcNow }
        };

        _sessions.ListAsync(Arg.Any<Expression<Func<UserSession, bool>>>(), Arg.Any<CancellationToken>())
                 .Returns(sessionsList);

        var query = new GetActiveSessionsQuery(10L);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);

        result.Value![0].IsCurrent.Should().BeTrue();  // active & matching IP
        result.Value[1].IsCurrent.Should().BeFalse(); // active & non-matching IP
        result.Value[2].IsCurrent.Should().BeFalse(); // inactive & matching IP
    }
}
