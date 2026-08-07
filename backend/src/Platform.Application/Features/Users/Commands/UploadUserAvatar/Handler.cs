using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Platform.Application.Common.Abstractions;
using Platform.Application.Common.Contracts.Storage;
using Platform.Application.Features.Users.Dtos;
using Platform.Domain.Results;

namespace Platform.Application.Features.Users.Commands.UploadUserAvatar;

public sealed class UploadUserAvatarHandler : IRequestHandler<UploadUserAvatarCommand, Result<AvatarUploadResponse>>
{
    private readonly IObjectStorage _storage;
    private readonly ICurrentUser _current;

    public UploadUserAvatarHandler(IObjectStorage storage, ICurrentUser current)
    {
        _storage = storage;
        _current = current;
    }

    public async Task<Result<AvatarUploadResponse>> Handle(UploadUserAvatarCommand cmd, CancellationToken ct)
    {
        if (_current.UserId is null)
            return Error.Unauthorized("auth.unauthenticated", "You must be logged in.");

        var ext = Path.GetExtension(cmd.FileName).ToLowerInvariant();
        var key = $"avatars/user_{_current.UserId.Value}_{Guid.NewGuid():N}{ext}";

        var request = new UploadUrlRequest(key, cmd.ContentType, cmd.SizeBytes);
        var urlResult = await _storage.GetUploadUrlAsync(request, ct);

        if (!urlResult.IsSuccess)
            return urlResult.Error;

        return new AvatarUploadResponse(
            Key: key,
            UploadUrl: urlResult.Value!.Url,
            ExpiresAt: urlResult.Value.ExpiresAt);
    }
}
