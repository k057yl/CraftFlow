using CraftFlow.Api.Core.Persistence;
using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.Domain;
using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.Services;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.BackgroundWorkers;

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
                var lifecycleService = scope.ServiceProvider.GetRequiredService<SubscriptionLifecycleService>();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                bool downgraded = await lifecycleService.DowngradeExpiredModulesAsync(tenantId: null, stoppingToken);

                var nowUtc = DateTime.UtcNow;
                var expiredCount = await dbContext.Set<TenantSubscription>()
                    .IgnoreQueryFilters()
                    .Where(s => (s.State == SubscriptionState.Active || s.State == SubscriptionState.Trial)
                             && s.ExpiresAtUtc.HasValue
                             && s.ExpiresAtUtc.Value <= nowUtc)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.State, SubscriptionState.Expired), stoppingToken);

                if (downgraded || expiredCount > 0)
                {
                    _logger.LogInformation("SUBSCRIPTION_EXPIRATION_BATCH_COMPLETED: Downgraded={Downgraded}, ExpiredCount={Count}", downgraded, expiredCount);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SUBSCRIPTION_EXPIRATION_CHECK_FAILED");
            }

            await Task.Delay(TimeSpan.FromHours(CHECK_INTERVAL_HOURS), stoppingToken);
        }
    }
}