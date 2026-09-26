using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Subscriptions.Domain;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Common.BackgroundWorkers;

public class SubscriptionExpirationWorker : BackgroundService
{
    private const int CHECK_INTERVAL_HOURS = 1;
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

                var nowUtc = DateTime.UtcNow;

                var expiredCount = await dbContext.Set<TenantSubscription>()
                    .Where(s => (s.State == SubscriptionState.Active || s.State == SubscriptionState.Trial)
                             && s.ExpiresAtUtc.HasValue
                             && s.ExpiresAtUtc.Value <= nowUtc)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.State, SubscriptionState.Expired), stoppingToken);

                if (expiredCount > 0)
                {
                    _logger.LogInformation("SUBSCRIPTION_EXPIRATION_BATCH_COMPLETED: ExpiredCount={Count}", expiredCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SUBSCRIPTION_EXPIRATION_CHECK_FAILED");
            }

            await Task.Delay(TimeSpan.FromHours(CHECK_INTERVAL_HOURS), stoppingToken);
        }
    }
}