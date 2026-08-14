namespace Platform.Infrastructure.Judge;

public class JudgeOptions
{
    public const string SectionName = "Judge";

    public string BaseUrl { get; set; } = "https://api.onlinecompiler.io/";
    public string ApiKey { get; set; } = "";
    public int TimeoutSeconds { get; set; } = 30;
}
