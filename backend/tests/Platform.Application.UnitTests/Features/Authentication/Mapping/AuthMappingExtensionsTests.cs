using System;
using System.Collections.Generic;
using FluentAssertions;
using Platform.Application.Common.Contracts.Authentication;
using Platform.Application.Features.Authentication.Mapping;
using Platform.Domain.Entities;
using Xunit;

namespace Platform.Application.UnitTests.Features.Authentication.Mapping;

public class AuthMappingExtensionsTests
{
    [Fact]
    public void ToRegisterResponse_ShouldMapUserFields()
    {
        // Arrange
        var user = new User
        {
            Id = 10,
            Email = "test@example.com",
            FullName = "Test User",
            EmailConfirmed = false
        };

        // Act
        var response = user.ToRegisterResponse();

        // Assert
        response.UserId.Should().Be(10);
        response.Email.Should().Be("test@example.com");
        response.FullName.Should().Be("Test User");
        response.EmailVerificationRequired.Should().BeTrue();
    }

    [Fact]
    public void ToLoginResponse_ShouldMapUserAndIssuedTokens()
    {
        // Arrange
        var user = new User { Id = 10, Email = "test@example.com", FullName = "Test User" };
        var now = DateTimeOffset.UtcNow;
        var issued = new IssuedTokens("access", now.AddMinutes(15), "refresh", now.AddDays(7), "jti-123");

        // Act
        var response = user.ToLoginResponse("Admin", issued);

        // Assert
        response.UserId.Should().Be(10);
        response.Role.Should().Be("Admin");
        response.AccessToken.Should().Be("access");
        response.RefreshToken.Should().Be("refresh");
        response.AccessTokenExpiresAt.Should().Be(issued.AccessTokenExpiresAt);
    }

    [Fact]
    public void ToCurrentUserResponse_ShouldUseResolvedRoleName()
    {
        // Arrange
        var student = new User { Id = 1, RoleId = 1, Email = "s@test.com", FullName = "Student" };
        var teacher = new User { Id = 2, RoleId = 2, Email = "t@test.com", FullName = "Teacher" };
        var admin = new User { Id = 3, RoleId = 3, Email = "a@test.com", FullName = "Admin" };

        // Act & Assert
        student.ToCurrentUserResponse("Student").Role.Should().Be("Student");
        teacher.ToCurrentUserResponse("Teacher").Role.Should().Be("Teacher");
        admin.ToCurrentUserResponse("Admin").Role.Should().Be("Admin");
    }

    [Fact]
    public void ToSessionResponse_ShouldSetIsCurrentTrueOnlyWhenActiveAndIpMatches()
    {
        // Arrange
        var activeMatch = new UserSession { Id = 1, IpAddress = "127.0.0.1", IsActive = true };
        var activeDiff = new UserSession { Id = 2, IpAddress = "10.0.0.1", IsActive = true };
        var inactiveMatch = new UserSession { Id = 3, IpAddress = "127.0.0.1", IsActive = false };

        // Act & Assert
        activeMatch.ToSessionResponse("127.0.0.1").IsCurrent.Should().BeTrue();
        activeDiff.ToSessionResponse("127.0.0.1").IsCurrent.Should().BeFalse();
        inactiveMatch.ToSessionResponse("127.0.0.1").IsCurrent.Should().BeFalse();
    }
}
