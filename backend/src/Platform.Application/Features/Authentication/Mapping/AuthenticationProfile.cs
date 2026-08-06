using AutoMapper;
using Platform.Application.Common.Mapping;
using Platform.Application.Features.Authentication.Dtos;
using Platform.Domain.Entities;

namespace Platform.Application.Features.Authentication.Mapping;

/// <summary>
/// AutoMapper profile for the Authentication feature.
/// Maps Domain entities → Application response records (from Dtos/AuthResponses.cs).
/// Discovered automatically by AddAutoMapper(assembly) in DI.
/// </summary>
public sealed class AuthenticationProfile : MappingProfile
{
    protected override void ConfigureMappings()
    {
        // ── User → CurrentUserResponse ────────────────────────────────────
        CreateMap<User, CurrentUserResponse>()
            .ConstructUsing((src, _) => new CurrentUserResponse(
                src.Id,
                src.Email,
                src.FullName,
                src.Phone,
                src.RoleId switch { 2 => "Teacher", 3 => "Admin", _ => "Student" },
                src.EmailConfirmed,
                src.LastLogin,
                src.CreatedAt));

        // ── UserSession → SessionResponse ─────────────────────────────────
        // IsCurrent is resolved at query time (requires ICurrentUser IP context);
        // default it to false here — the handler overrides it inline.
        CreateMap<UserSession, SessionResponse>()
            .ConstructUsing((src, _) => new SessionResponse(
                src.Id,
                src.DeviceInfo ?? "Unknown Device",
                src.IpAddress  ?? "Unknown",
                src.LoginAt,
                src.LogoutAt,
                src.IsActive,
                IsCurrent: false));
    }
}
