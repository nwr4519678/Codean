namespace Platform.Infrastructure.Judge;

public class JudgeOptions
{
    public const string SectionName = "Judge";

    public string BaseUrl { get; set; } = "http://localhost:5002";
    public int TimeoutSeconds { get; set; } = 30;
}
