namespace CraftFlow.Api.Common.Infrastructure.Notifications;

public interface INotificationQueue
{
    ValueTask QueueNotificationAsync(NotificationJob job, CancellationToken cancellationToken = default);
    IAsyncEnumerable<NotificationJob> ReadAllAsync(CancellationToken cancellationToken);
}