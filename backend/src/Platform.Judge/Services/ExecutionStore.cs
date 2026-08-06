using System;
using System.Collections.Concurrent;
using Platform.Judge.Contracts;
using Platform.Judge.Models;

namespace Platform.Judge.Services;

public class ExecutionStore
{
    private readonly ConcurrentDictionary<string, ExecutionStatusResponse> _statusStore = new();
    private readonly ConcurrentDictionary<string, ExecutionResult> _resultStore = new();

    public bool TryAddOrGetSubmission(string executionId, long submissionId, out ExecutionStatusResponse statusResponse)
    {
        var newStatus = new ExecutionStatusResponse(
            executionId,
            submissionId,
            ExecutionStatus.Queued,
            DateTime.UtcNow
        );

        if (_statusStore.TryAdd(executionId, newStatus))
        {
            statusResponse = newStatus;
            return true;
        }

        statusResponse = _statusStore[executionId];
        return false;
    }

    public void UpdateStatus(string executionId, ExecutionStatus status, DateTime? completedAt = null)
    {
        if (_statusStore.TryGetValue(executionId, out var existing))
        {
            _statusStore[executionId] = existing with
            {
                Status = status,
                CompletedAt = completedAt ?? existing.CompletedAt
            };
        }
    }

    public ExecutionStatusResponse? GetStatus(string executionId)
    {
        return _statusStore.TryGetValue(executionId, out var status) ? status : null;
    }

    public void SaveResult(string executionId, ExecutionResult result)
    {
        _resultStore[executionId] = result;
        UpdateStatus(executionId, result.Status, DateTime.UtcNow);
    }

    public ExecutionResult? GetResult(string executionId)
    {
        return _resultStore.TryGetValue(executionId, out var res) ? res : null;
    }
}
