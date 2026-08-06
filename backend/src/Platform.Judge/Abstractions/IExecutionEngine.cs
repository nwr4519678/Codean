using System.Threading;
using System.Threading.Tasks;
using Platform.Judge.Models;

namespace Platform.Judge.Abstractions;

public interface IExecutionEngine
{
    string EngineName { get; }
    bool CanExecute(string language);
    Task<ExecutionResult> ExecuteAsync(ExecutionRequest request, CancellationToken cancellationToken = default);
}
