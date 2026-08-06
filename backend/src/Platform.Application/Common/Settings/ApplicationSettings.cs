namespace Platform.Application.Common.Settings;

public sealed class ApplicationSettings
{
    public string AppName { get; set; } = "Platform";
    public string Version { get; set; } = "1.0.0";
    public string SupportEmail { get; set; } = "support@platform.app";
    public bool EnableRegistration { get; set; } = true;
}
