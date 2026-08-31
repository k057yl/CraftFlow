using CraftFlow.Api.Common.Constants;
using CraftFlow.Api.Common.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Common.BackgroundWorkers;

public class LowStockMonitorWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LowStockMonitorWorker> _logger;
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(CoreConstants.InventoryThresholds.MONITOR_INTERVAL_MINUTES);

    public LowStockMonitorWorker(IServiceProvider serviceProvider, ILogger<LowStockMonitorWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Low Stock Monitor Background Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var lowStockLots = await dbContext.StockLots
                    .IgnoreQueryFilters()
                    .Where(s => s.Quantity < CoreConstants.InventoryThresholds.LOW_STOCK_MIN_QUANTITY)
                    .ToListAsync(stoppingToken);

                if (lowStockLots.Count > 0)
                {
                    _logger.LogWarning(
                        "BACKGROUND WORKER ALERT: Found {Count} stock lots with quantity below threshold!",
                        lowStockLots.Count
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking low stock items.");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }
}