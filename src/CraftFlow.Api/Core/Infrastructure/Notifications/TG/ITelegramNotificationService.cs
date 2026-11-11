namespace CraftFlow.Api.Core.Infrastructure.Notifications.TG;

public interface ITelegramNotificationService
{
    Task SendAdminNotificationAsync(string message, CancellationToken cancellationToken = default);
}