using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Platform.Judge.Models;

namespace Platform.Judge.Worker;

/// <summary>
/// Internal in-memory, non-durable channel queue for buffering code execution requests
/// inside Platform.Judge service worker.
/// </summary>
public class ExecutionQueue
{
    private readonly Channel<ExecutionRequest> _channel;

    public ExecutionQueue(int capacity = 1000)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            SingleWriter = false,
            SingleReader = false,
            FullMode = BoundedChannelFullMode.Wait
        };
        _channel = Channel.CreateBounded<ExecutionRequest>(options);
    }

    public async ValueTask EnqueueAsync(ExecutionRequest request, CancellationToken cancellationToken = default)
    {
        await _channel.Writer.WriteAsync(request, cancellationToken);
    }

    public async ValueTask<ExecutionRequest> DequeueAsync(CancellationToken cancellationToken = default)
    {
        return await _channel.Reader.ReadAsync(cancellationToken);
    }
}
