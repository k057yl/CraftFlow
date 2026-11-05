using System.Threading.Channels;

namespace CraftFlow.Api.Common.Infrastructure.Notifications;

public sealed class InMemoryNotificationQueue : INotificationQueue
{
    private readonly Channel<NotificationJob> _channel;

    public InMemoryNotificationQueue(int capacity = 1000)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        };
        _channel = Channel.CreateBounded<NotificationJob>(options);
    }

    public ValueTask QueueNotificationAsync(NotificationJob job, CancellationToken cancellationToken = default)
    {
        return _channel.Writer.WriteAsync(job, cancellationToken);
    }

    public IAsyncEnumerable<NotificationJob> ReadAllAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
}