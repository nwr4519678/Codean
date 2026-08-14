using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Platform.Application.Common.Contracts.Judge;

namespace Platform.Infrastructure.Judge;

/// <summary>
/// Thin adapter over OnlineCompiler.io. The platform never compiles or executes
/// student code locally; the provider owns sandboxing and resource limits.
/// </summary>
public sealed class OnlineCompilerClient(
    HttpClient httpClient,
    IOptions<JudgeOptions> options,
    ILogger<OnlineCompilerClient> logger) : IJudgeService
{
    private readonly JudgeOptions _options = options.Value;

    public async Task<CodeExecutionResult> ExecuteAsync(
        CodeExecutionRequest request,
        CancellationToken cancellationToken = default)
    {
        var compiler = MapCompiler(request.Language);
        var testCases = request.TestCases is { Count: > 0 }
            ? request.TestCases
            : [new TestCaseDto(request.StandardInput ?? "", "")];
        var results = new List<TestCaseResultDto>(testCases.Count);
        var stopwatch = Stopwatch.StartNew();
        string? compilationOutput = null;
        string? standardOutput = null;
        string? standardError = null;
        long memory = 0;
        double executionMs = 0;
        int? exitCode = null;
        Verdict? firstFailure = null;

        try
        {
            foreach (var testCase in testCases)
            {
                using var response = await httpClient.PostAsJsonAsync(
                    "api/run-code-sync/",
                    new OnlineCompilerRequest(compiler, request.SourceCode, testCase.Input),
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning("OnlineCompiler returned HTTP {StatusCode}.", response.StatusCode);
                    return Failed(request, InfrastructureFailure.ProviderUnavailable, "External compiler API rejected the request.");
                }

                var provider = await response.Content.ReadFromJsonAsync<OnlineCompilerResponse>(cancellationToken);
                if (provider is null)
                    return Failed(request, InfrastructureFailure.ProviderUnavailable, "External compiler returned an empty response.");

                var actualOutput = provider.Output ?? "";
                var error = string.IsNullOrWhiteSpace(provider.Error) ? null : provider.Error;
                var passed = string.Equals(provider.Status, "success", StringComparison.OrdinalIgnoreCase)
                    && (string.IsNullOrWhiteSpace(testCase.ExpectedOutput) ||
                        Normalize(actualOutput) == Normalize(testCase.ExpectedOutput));
                var verdict = provider.Status.Equals("success", StringComparison.OrdinalIgnoreCase)
                    ? (passed ? Verdict.Accepted : Verdict.WrongAnswer)
                    : Verdict.CompilationError;
                firstFailure ??= passed ? null : verdict;
                compilationOutput ??= error;
                standardOutput = actualOutput;
                standardError = error;
                memory = long.TryParse(provider.Memory, out var parsedMemory) ? parsedMemory : memory;
                executionMs += double.TryParse(provider.Time, out var parsedTime) ? parsedTime * 1000 : 0;
                exitCode = provider.ExitCode;
                results.Add(new TestCaseResultDto(results.Count, passed, actualOutput, error, executionMs, memory, verdict == Verdict.Accepted ? null : verdict.ToString(), testCase.IsHidden));
            }

            var passedCount = results.Count(x => x.Passed);
            var verdictResult = passedCount == results.Count ? Verdict.Accepted : firstFailure ?? Verdict.RuntimeError;
            return new CodeExecutionResult(request.ExecutionId, request.SubmissionId, ExecutionStatus.Completed,
                verdictResult, InfrastructureFailure.None, passedCount, results.Count, results,
                compilationOutput, standardOutput, standardError, exitCode, stopwatch.Elapsed.TotalMilliseconds, memory,
                verdictResult == Verdict.Accepted ? null : verdictResult.ToString());
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "OnlineCompiler execution failed for {ExecutionId}.", request.ExecutionId);
            return Failed(request, InfrastructureFailure.ProviderUnavailable, "External compiler API is unavailable.");
        }
    }

    private static CodeExecutionResult Failed(CodeExecutionRequest request, InfrastructureFailure failure, string reason) =>
        new(request.ExecutionId, request.SubmissionId, ExecutionStatus.Failed, null, failure, 0, 0, [], null, null, null, null, 0, 0, reason);

    private static string MapCompiler(string language) => language.Trim().ToLowerInvariant() switch
    {
        "python" or "python3" => "python-3.14",
        "c#" or "csharp" or "cs" => "dotnet-csharp-9",
        "javascript" or "js" or "typescript" or "deno" => "typescript-deno",
        _ => throw new ArgumentException($"Unsupported executable language: {language}")
    };

    private static string Normalize(string value) => value.Trim().Replace("\r\n", "\n");

    private sealed record OnlineCompilerRequest(string compiler, string code, string input);

    private sealed record OnlineCompilerResponse(
        string? Output,
        string? Error,
        string Status,
        [property: JsonPropertyName("exit_code")] int? ExitCode,
        string? Time,
        string? Memory);
}
