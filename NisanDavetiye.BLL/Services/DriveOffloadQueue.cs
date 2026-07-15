using System.Threading.Channels;

namespace NisanDavetiye.BLL.Services;

/// <summary>Drive'a aktarılacak galeri kayıtlarını grup (batch) halinde tutan kuyruk.</summary>
public interface IDriveOffloadQueue
{
    ValueTask EnqueueBatchAsync(IReadOnlyList<int> galeriResmiIds, CancellationToken cancellationToken = default);
    IAsyncEnumerable<IReadOnlyList<int>> DequeueAllAsync(CancellationToken cancellationToken);
}

public class DriveOffloadQueue : IDriveOffloadQueue
{
    private readonly Channel<IReadOnlyList<int>> _channel =
        Channel.CreateUnbounded<IReadOnlyList<int>>(new UnboundedChannelOptions { SingleReader = true });

    public ValueTask EnqueueBatchAsync(IReadOnlyList<int> galeriResmiIds, CancellationToken cancellationToken = default)
    {
        if (galeriResmiIds.Count == 0)
            return ValueTask.CompletedTask;

        return _channel.Writer.WriteAsync(galeriResmiIds, cancellationToken);
    }

    public IAsyncEnumerable<IReadOnlyList<int>> DequeueAllAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAllAsync(cancellationToken);
}
