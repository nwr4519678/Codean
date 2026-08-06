using AutoMapper;

namespace Platform.Application.Common.Mapping;

/// <summary>
/// Base class for every feature's AutoMapper profile.
/// AutoMapper discovers all subclasses automatically via assembly scanning in DependencyInjection.cs.
/// </summary>
public abstract class MappingProfile : Profile
{
    protected MappingProfile() => ConfigureMappings();

    /// <summary>Override in subclasses to declare type maps.</summary>
    protected abstract void ConfigureMappings();
}
