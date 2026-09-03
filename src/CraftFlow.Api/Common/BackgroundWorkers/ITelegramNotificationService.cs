namespace CraftFlow.Api.Common.BackgroundWorkers;
public interface ITelegramNotificationService
{
    Task SendAdminNotificationAsync(string message, CancellationToken cancellationToken = default);
}
