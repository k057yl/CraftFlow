using CraftFlow.Api.Common.BackgroundWorkers;
using CraftFlow.Api.Infrastructure.Services;

namespace CraftFlow.Api.Common.Infrastructure.Notifications;

public sealed class NotificationBackgroundWorker : BackgroundService
{
    private readonly INotificationQueue _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationBackgroundWorker> _logger;

    public NotificationBackgroundWorker(
        INotificationQueue queue,
        IServiceProvider serviceProvider,
        ILogger<NotificationBackgroundWorker> logger)
    {
        _queue = queue;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var job in _queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();

                if (job.Type == NotificationType.Email)
                {
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                    await emailService.SendEmailAsync(job.Recipient, job.Subject, job.Body);
                }
                else if (job.Type == NotificationType.Telegram)
                {
                    var telegramService = scope.ServiceProvider.GetRequiredService<ITelegramNotificationService>();
                    await telegramService.SendAdminNotificationAsync(job.Body, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка отправки фонового уведомления {Type} для {Recipient}", job.Type, job.Recipient);
            }
        }
    }
}