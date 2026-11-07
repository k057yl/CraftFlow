namespace CraftFlow.Api.Core.BackgroundWorkers;

public interface ITelegramNotificationService
{
    Task SendAdminNotificationAsync(string message, CancellationToken cancellationToken = default);
}