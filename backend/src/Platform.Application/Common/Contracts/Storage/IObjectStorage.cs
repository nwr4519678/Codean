using Platform.Domain.Results;

namespace Platform.Application.Common.Contracts.Storage;

public sealed record UploadUrlRequest(
    string Key,
    string ContentType,
    long SizeBytes);

public sealed record UploadUrlResult(
    string Url,
    string Method,
    IReadOnlyDictionary<string, string> Headers,
    DateTimeOffset ExpiresAt);

public sealed record SignedUrlResult(
    string Url,
    DateTimeOffset ExpiresAt);

public interface IObjectStorage
{
    Task<Result<UploadUrlResult>> GetUploadUrlAsync(UploadUrlRequest request, CancellationToken ct = default);
    Task<Result<SignedUrlResult>> GetDownloadUrlAsync(string key, TimeSpan ttl, CancellationToken ct = default);
    Task<bool> DeleteAsync(string key, CancellationToken ct = default);
    Task<bool> ExistsAsync(string key, CancellationToken ct = default);
}
