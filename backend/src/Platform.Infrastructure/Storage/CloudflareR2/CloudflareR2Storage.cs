using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Platform.Application.Common.Contracts.Storage;
using Platform.Domain.Results;

namespace Platform.Infrastructure.Storage.CloudflareR2;

public sealed class R2Options
{
    public string AccountId { get; set; } = "";
    public string AccessKey { get; set; } = "";
    public string SecretKey { get; set; } = "";
    public string Bucket { get; set; } = "platform";
    public string? PublicBaseUrl { get; set; }
}

/// <summary>
/// Cloudflare R2 uses the S3 API. We keep everything private and use
/// pre-signed URLs for both upload (PUT) and download (GET).
/// </summary>
public sealed class CloudflareR2Storage : IObjectStorage
{
    private readonly IAmazonS3 _s3;
    private readonly R2Options _opt;
    private readonly ILogger<CloudflareR2Storage> _logger;

    public CloudflareR2Storage(IOptions<R2Options> opt, ILogger<CloudflareR2Storage> logger)
    {
        _opt = opt.Value;
        _logger = logger;
        var cfg = new AmazonS3Config
        {
            ServiceURL = $"https://{_opt.AccountId}.r2.cloudflarestorage.com",
            AuthenticationRegion = "auto",
            ForcePathStyle = true
        };
        _s3 = new AmazonS3Client(_opt.AccessKey, _opt.SecretKey, cfg);
    }

    public async Task<Result<UploadUrlResult>> GetUploadUrlAsync(UploadUrlRequest request, CancellationToken ct = default)
    {
        try
        {
            var expires = DateTimeOffset.UtcNow.AddMinutes(15);
            var presign = _s3.GetPreSignedURL(new GetPreSignedUrlRequest
            {
                BucketName = _opt.Bucket,
                Key = request.Key,
                Verb = HttpVerb.PUT,
                Expires = expires.UtcDateTime,
                ContentType = request.ContentType
            });
            var headers = new Dictionary<string, string> { ["Content-Type"] = request.ContentType };
            return new UploadUrlResult(presign, "PUT", headers, expires);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "R2 upload URL failed for {Key}", request.Key);
            return Error.Provider("r2.upload_url_failed", "Could not generate upload URL.");
        }
    }

    public async Task<Result<SignedUrlResult>> GetDownloadUrlAsync(string key, TimeSpan ttl, CancellationToken ct = default)
    {
        try
        {
            var url = _s3.GetPreSignedURL(new GetPreSignedUrlRequest
            {
                BucketName = _opt.Bucket,
                Key = key,
                Verb = HttpVerb.GET,
                Expires = DateTimeOffset.UtcNow.Add(ttl).UtcDateTime
            });
            return new SignedUrlResult(url, DateTimeOffset.UtcNow.Add(ttl));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "R2 download URL failed for {Key}", key);
            return Error.Provider("r2.download_url_failed", "Could not generate download URL.");
        }
    }

    public async Task<bool> DeleteAsync(string key, CancellationToken ct = default)
    {
        try
        {
            await _s3.DeleteObjectAsync(_opt.Bucket, key, ct);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "R2 delete failed for {Key}", key);
            return false;
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken ct = default)
    {
        try
        {
            await _s3.GetObjectMetadataAsync(_opt.Bucket, key, ct);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
