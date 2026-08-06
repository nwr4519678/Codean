using System.Threading.Tasks;
using Platform.Infrastructure.Messaging;

namespace Platform.Infrastructure.Jobs;

/// <summary>
/// Hangfire recurring job for outbox processing.
///
/// SRP: This class only knows when to trigger processing.
/// The actual processing logic is in <see cref="OutboxProcessor"/>.
/// This separation means ProcessAsync() can be triggered by any scheduler
/// (Hangfire, Worker Service, Azure Functions) without touching the processor.
/// </summary>
internal sealed class ProcessOutboxJob(OutboxProcessor processor)
{
    /// <summary>Invoked by Hangfire every 30 seconds.</summary>
    public Task ExecuteAsync() => processor.ProcessAsync();
}
