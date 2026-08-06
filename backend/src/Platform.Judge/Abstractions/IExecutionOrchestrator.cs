using System.Threading;
using System.Threading.Tasks;
using Platform.Judge.Models;

namespace Platform.Judge.Abstractions;

public interface IExecutionOrchestrator
{
    Task<ExecutionResult> ExecuteAsync(ExecutionRequest request, CancellationToken cancellationToken = default);
}
