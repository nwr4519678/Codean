using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Platform.Application.Common.Contracts.Judge;

namespace Platform.Infrastructure.Judge;

public class JudgeHttpClient : IJudgeService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<JudgeHttpClient> _logger;

    public JudgeHttpClient(HttpClient httpClient, ILogger<JudgeHttpClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ExecutionSubmissionResponse> SubmitAsync(CodeExecutionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/judge/executions", request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ExecutionSubmissionResponse>(cancellationToken: cancellationToken);
            return result ?? new ExecutionSubmissionResponse(
                request.ExecutionId,
                request.SubmissionId,
                ExecutionStatus.Failed,
                DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting execution request {ExecutionId} to Judge service", request.ExecutionId);
            return new ExecutionSubmissionResponse(
                request.ExecutionId,
                request.SubmissionId,
                ExecutionStatus.Failed,
                DateTime.UtcNow);
        }
    }

    public async Task<ExecutionStatusResponse?> GetStatusAsync(string executionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/judge/executions/{executionId}/status", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<ExecutionStatusResponse>(cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying status for execution {ExecutionId} from Judge service", executionId);
            return null;
        }
    }

    public async Task<CodeExecutionResult?> GetResultAsync(string executionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/judge/executions/{executionId}/result", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<CodeExecutionResult>(cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching result for execution {ExecutionId} from Judge service", executionId);
            return null;
        }
    }
}
