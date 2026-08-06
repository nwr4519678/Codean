namespace Platform.Application.Common.Contracts.Authentication;

/// <summary>
/// Password hashing abstraction. Default implementation is BCrypt (cost 12) in Infrastructure.
/// Exposed in the Application layer so handlers can hash/verify without referencing BCrypt directly.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

/// <summary>
/// TOTP abstraction. Default implementation lives in Infrastructure.
/// </summary>
public interface ITotpService
{
    string GenerateSecret();
    string GenerateQrUri(string email, string secret);
    bool Verify(string secret, string code);
    string[] GenerateBackupCodes(int count = 10);
    string HashBackupCode(string code);
}
