using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Platform.Application.Common.Abstractions;
using Platform.Application.Common.Contracts.Judge;
using Platform.Domain.Entities;

namespace Platform.Infrastructure.Jobs;

/// <summary>
/// Hangfire recurring job that reconciles "Queued" or "Running" CodingSubmissions
/// against the Judge Engine. Prevents stuck submissions if a client never polls.
///
/// Runs every 2 minutes via InfrastructureJobScheduler.
/// </summary>
internal sealed class ProcessJudgeResultsJob(
    IRepository<CodingSubmission> submissions,
    IJudgeService judgeService,
    IUnitOfWork uow,
    ILogger<ProcessJudgeResultsJob> logger)
{
    private const int BatchSize = 50;

    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        var pending = await submissions.ListAsync(
            s => (s.Status == "Queued" || s.Status == "Running")
              && s.ExecutionResult != null,
            ct);

        if (pending.Count == 0) return;

        logger.LogInformation("ProcessJudgeResultsJob: reconciling {Count} pending submissions.", pending.Count);

        var batch = pending.Take(BatchSize).ToList();
        int reconciled = 0;

        foreach (var submission in batch)
        {
            try
            {
                var executionId = submission.ExecutionResult!;
                var result = await judgeService.GetResultAsync(executionId, ct);

                if (result is null ||
                    result.Status is not (ExecutionStatus.Completed or ExecutionStatus.Failed))
                    continue;

                var passed = result.PassedTestCases;
                var total  = result.TotalTestCases;

                submission.Status = result.Status.ToString();
                submission.Score  = total > 0
                    ? Math.Round((decimal)passed / total * 100, 2)
                    : 0;

                submissions.Update(submission);
                reconciled++;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "ProcessJudgeResultsJob: failed to reconcile submission {Id}.", submission.Id);
            }
        }

        if (reconciled > 0)
        {
            await uow.SaveChangesAsync(ct);
            logger.LogInformation("ProcessJudgeResultsJob: reconciled {Count} submissions.", reconciled);
        }
    }
}
