using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Platform.Judge.Engines.Judge0;

public record Judge0SubmissionRequest(
    [property: JsonPropertyName("source_code")] string SourceCode,
    [property: JsonPropertyName("language_id")] int LanguageId,
    [property: JsonPropertyName("stdin")] string? Stdin = null,
    [property: JsonPropertyName("expected_output")] string? ExpectedOutput = null,
    [property: JsonPropertyName("cpu_time_limit")] double? CpuTimeLimit = null,
    [property: JsonPropertyName("memory_limit")] long? MemoryLimit = null
);

public record Judge0StatusResponse(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("description")] string Description
);

public record Judge0SubmissionResponse(
    [property: JsonPropertyName("token")] string? Token,
    [property: JsonPropertyName("stdout")] string? Stdout,
    [property: JsonPropertyName("stderr")] string? Stderr,
    [property: JsonPropertyName("compile_output")] string? CompileOutput,
    [property: JsonPropertyName("message")] string? Message,
    [property: JsonPropertyName("exit_code")] int? ExitCode,
    [property: JsonPropertyName("time")] string? Time,
    [property: JsonPropertyName("memory")] long? Memory,
    [property: JsonPropertyName("status")] Judge0StatusResponse? Status
);

public class Judge0Client
{
    private readonly HttpClient _httpClient;
    private readonly Judge0Options _options;
    private readonly ILogger<Judge0Client> _logger;

    public Judge0Client(HttpClient httpClient, IOptions<Judge0Options> options, ILogger<Judge0Client> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Judge0SubmissionResponse?> SubmitSubmissionAsync(
        Judge0SubmissionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Submit with wait=true for fast response, or fallback to token polling
            var url = "submissions?base64_encoded=false&wait=true";
            var response = await _httpClient.PostAsJsonAsync(url, request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Judge0SubmissionResponse>(cancellationToken: cancellationToken);
            }

            _logger.LogWarning("Judge0 submission post failed with status code {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HTTP exception when connecting to Judge0 API");
            return null;
        }
    }
}
