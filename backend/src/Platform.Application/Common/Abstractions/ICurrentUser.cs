namespace Platform.Application.Common.Abstractions;

public interface ICurrentUser
{
    long? UserId { get; }
    string? Email { get; }
    string? IpAddress { get; }
    string? UserAgent { get; }
}
