namespace Platform.Shared.Results;

/// <summary>
/// A typed error carrying a stable code, a human-readable message, and
/// an HTTP-style status hint for the API boundary.
/// </summary>
public sealed record Error(string Code, string Message, ErrorType Type = ErrorType.Failure, IReadOnlyDictionary<string, object?>? Metadata = null)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    public static Error Validation(string code, string message, IReadOnlyDictionary<string, object?>? meta = null) =>
        new(code, message, ErrorType.Validation, meta);

    public static Error NotFound(string code, string message) =>
        new(code, message, ErrorType.NotFound);

    public static Error Conflict(string code, string message) =>
        new(code, message, ErrorType.Conflict);

    public static Error Forbidden(string code, string message) =>
        new(code, message, ErrorType.Forbidden);

    public static Error Unauthorized(string code, string message) =>
        new(code, message, ErrorType.Unauthorized);

    public static Error SubscriptionRequired(string code, string message) =>
        new(code, message, ErrorType.SubscriptionRequired);

    public static Error Provider(string code, string message) =>
        new(code, message, ErrorType.Provider);

    public static Error Internal(string code, string message) =>
        new(code, message, ErrorType.Internal);
}

public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Forbidden = 4,
    Unauthorized = 5,
    SubscriptionRequired = 6,
    Provider = 7,
    Internal = 8
}
