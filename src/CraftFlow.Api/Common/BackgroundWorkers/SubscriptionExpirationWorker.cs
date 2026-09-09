using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Subscriptions.Domain;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Common.BackgroundWorkers;

public class SubscriptionExpirationWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SubscriptionExpirationWorker> _logger;

    public SubscriptionExpirationWorker(
        IServiceProvider serviceProvider,
        ILogger<SubscriptionExpirationWorker> logger)
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

                var expiredSubscriptions = await dbContext.Set<TenantSubscription>()
                    .Where(s => (s.State == SubscriptionState.Active || s.State == SubscriptionState.Trial)
                             && s.ExpiresAtUtc.HasValue
                             && s.ExpiresAtUtc.Value <= DateTime.UtcNow)
                    .ToListAsync(stoppingToken);

                foreach (var subscription in expiredSubscriptions)
                {
                    subscription.Expire();
                }

                if (expiredSubscriptions.Count > 0)
                {
                    await dbContext.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Просрочено {Count} подписок", expiredSubscriptions.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке истечения срока подписок");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}