using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using OtpNet;
using Platform.Application.Common.Contracts.Authentication;

namespace Platform.Infrastructure.Security;

public sealed class TotpService : ITotpService
{
    public string GenerateSecret()
    {
        var key = KeyGeneration.GenerateRandomKey(20);
        return Base32Encoding.ToString(key);
    }

    public string GenerateQrUri(string email, string secret)
    {
        var issuer = Uri.EscapeDataString("Platform");
        var user = Uri.EscapeDataString(email);
        return $"otpauth://totp/{issuer}:{user}?secret={secret}&issuer={issuer}&digits=6";
    }

    public bool Verify(string secret, string code)
    {
        if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(code)) return false;
        try
        {
            var bytes = Base32Encoding.ToBytes(secret);
            var totp = new Totp(bytes);
            return totp.VerifyTotp(code.Trim(), out _, VerificationWindow.RfcSpecifiedNetworkDelay);
        }
        catch
        {
            return false;
        }
    }

    public string[] GenerateBackupCodes(int count = 10)
    {
        var codes = new string[count];
        for (int i = 0; i < count; i++)
        {
            var raw = Convert.ToHexString(RandomNumberGenerator.GetBytes(4));
            codes[i] = $"{raw[..4]}-{raw[4..]}";
        }
        return codes;
    }

    public string HashBackupCode(string code)
    {
        var normalized = code.Replace("-", "").Trim().ToUpperInvariant();
        var bytes = Encoding.UTF8.GetBytes(normalized);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
