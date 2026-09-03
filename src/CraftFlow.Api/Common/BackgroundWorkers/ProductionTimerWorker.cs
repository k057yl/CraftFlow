using CraftFlow.Api.Common.BackgroundWorkers;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Production.Domain;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.BackgroundWorkers;

public class ProductionTimerWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ProductionTimerWorker> _logger;

    public ProductionTimerWorker(IServiceProvider serviceProvider, ILogger<ProductionTimerWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var telegramService = scope.ServiceProvider.GetService<ITelegramNotificationService>();

                if (telegramService != null)
                {
                    var now = DateTime.UtcNow;

                    var overdueBatches = await dbContext.Set<ProductionBatch>()
                        .Where(b => b.Status == BatchState.InProgress
                                 && !b.IsTelegramNotified
                                 && b.StartedAt.AddMinutes(b.TargetDurationMinutes) <= now)
                        .ToListAsync(stoppingToken);

                    foreach (var batch in overdueBatches)
                    {
                        var message = $"⏳ *Варка завершена!*\n\n" +
                                      $"*Партия:* `{batch.Name}`\n" +
                                      $"*Время в работе:* {batch.TargetDurationMinutes} мин.\n" +
                                      $"Пора сливать сыворотку или формировать головки!";

                        await telegramService.SendAdminNotificationAsync(message, stoppingToken);

                        batch.MarkTelegramNotified();
                    }

                    if (overdueBatches.Count > 0)
                    {
                        await dbContext.SaveChangesAsync(stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке таймеров варок.");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}