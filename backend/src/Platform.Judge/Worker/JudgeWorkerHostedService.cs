using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Platform.Judge.Contracts;
using Platform.Judge.Abstractions;
using Platform.Judge.Services;

namespace Platform.Judge.Worker;

public class JudgeWorkerHostedService : BackgroundService
{
    private readonly ExecutionQueue _queue;
    private readonly IExecutionOrchestrator _orchestrator;
    private readonly ExecutionStore _executionStore;
    private readonly ILogger<JudgeWorkerHostedService> _logger;

    public JudgeWorkerHostedService(
        ExecutionQueue queue,
        IExecutionOrchestrator orchestrator,
        ExecutionStore executionStore,
        ILogger<JudgeWorkerHostedService> logger)
    {
        _queue = queue;
        _orchestrator = orchestrator;
        _executionStore = executionStore;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Platform.Judge Worker Hosted Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var request = await _queue.DequeueAsync(stoppingToken);

                _logger.LogInformation("Processing queued job ExecutionId {ExecutionId} for Submission {SubmissionId}",
                    request.ExecutionId, request.SubmissionId);

                _executionStore.UpdateStatus(request.ExecutionId, ExecutionStatus.Running);

                var result = await _orchestrator.ExecuteAsync(request, stoppingToken);

                _executionStore.SaveResult(request.ExecutionId, result);

                _logger.LogInformation("Completed processing ExecutionId {ExecutionId}. Verdict: {Verdict}, Status: {Status}",
                    request.ExecutionId, result.Verdict, result.Status);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Platform.Judge Worker Hosted Service is stopping.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing execution job in JudgeWorkerHostedService.");
            }
        }
    }
}
