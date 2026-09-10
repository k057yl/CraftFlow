using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Infrastructure.BackgroundServices.Cleanup
{
    public class UnconfirmedUserCleanupWorker : BackgroundService
    {
        private const int CLEANUP_INTERVAL_HOURS = 24;

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<UnconfirmedUserCleanupWorker> _logger;

        public UnconfirmedUserCleanupWorker(IServiceProvider serviceProvider, ILogger<UnconfirmedUserCleanupWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("UnconfirmedUserCleanupWorker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessCleanupAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during unconfirmed users cleanup.");
                }

                await Task.Delay(TimeSpan.FromHours(CLEANUP_INTERVAL_HOURS), stoppingToken);
            }
        }

        private async Task ProcessCleanupAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var nowUtc = DateTime.UtcNow;

            var deletedCount = await dbContext.Users
                .IgnoreQueryFilters()
                .Where(u => u.OtpCodeHash != null && u.OtpExpiresAtUtc != null && u.OtpExpiresAtUtc < nowUtc)
                .ExecuteDeleteAsync(cancellationToken);

            if (deletedCount > 0)
            {
                _logger.LogInformation("Cleanup completed. Unconfirmed users sent to hell: {Count}", deletedCount);
            }
        }
    }
}