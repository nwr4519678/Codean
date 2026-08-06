namespace Platform.Judge.Engines.Judge0;

public class Judge0Options
{
    public const string SectionName = "Judge0";

    public string BaseUrl { get; set; } = "http://localhost:2358";
    public string? ApiKey { get; set; }
    public int TimeoutSeconds { get; set; } = 15;
    public int MaxPollingRetries { get; set; } = 30;
    public int PollingIntervalMs { get; set; } = 500;
}
