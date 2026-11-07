namespace CraftFlow.Api.Core.Infrastructure.Notifications;

public interface INotificationQueue
{
    ValueTask QueueNotificationAsync(NotificationJob job, CancellationToken cancellationToken = default);
    IAsyncEnumerable<NotificationJob> ReadAllAsync(CancellationToken cancellationToken);
}