using CraftFlow.Api.Common.Infrastructure.Services;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Infrastructure.Services;
using CraftFlow.Api.Modules.Identity.DeleteAccount;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Infrastructure.BackgroundServices.Cleanup;

public class IdentityCleanupWorker : BackgroundService
{
    private const int CLEANUP_INTERVAL_HOURS = 24;

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<IdentityCleanupWorker> _logger;

    public IdentityCleanupWorker(IServiceProvider serviceProvider, ILogger<IdentityCleanupWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("IDENTITY_CLEANUP_WORKER_STARTED");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupUnconfirmedUsersAsync(stoppingToken);
                await CleanupRetentionTenantsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IDENTITY_CLEANUP_EXECUTION_FAILED");
            }

            await Task.Delay(TimeSpan.FromHours(CLEANUP_INTERVAL_HOURS), stoppingToken);
        }
    }

    private async Task CleanupUnconfirmedUsersAsync(CancellationToken cancellationToken)
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
            _logger.LogInformation("UNCONFIRMED_USERS_PURGED: Count={Count}", deletedCount);
        }
    }

    private async Task CleanupRetentionTenantsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var nowUtc = DateTime.UtcNow;
        var sixMonthsAgo = nowUtc.AddMonths(-6);
        var twoYearsAgo = nowUtc.AddYears(-2);

        var expiredTenantIds = await dbContext.Organizations
            .IgnoreQueryFilters()
            .Where(o => !o.IsActive && o.IsSelfDeactivated && o.DeactivatedAtUtc <= twoYearsAgo)
            .Select(o => o.Id)
            .ToListAsync(cancellationToken);

        foreach (var tenantId in expiredTenantIds)
        {
            _logger.LogWarning("RETENTION_POLICY_PURGING_TENANT: TenantId={TenantId}", tenantId);
            await DeleteAccountHandler.HardDeleteTenantDataAsync(tenantId, dbContext, cancellationToken);
        }

        var tenantsToNotify = await dbContext.Organizations
            .IgnoreQueryFilters()
            .Where(o => !o.IsActive
                     && o.IsSelfDeactivated
                     && o.DeactivatedAtUtc <= sixMonthsAgo
                     && (o.LastRetentionNoticeSentAtUtc == null || o.LastRetentionNoticeSentAtUtc <= sixMonthsAgo))
            .ToListAsync(cancellationToken);

        foreach (var org in tenantsToNotify)
        {
            var ownerUser = await dbContext.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.TenantId == org.Id, cancellationToken);

            if (ownerUser != null && !string.IsNullOrWhiteSpace(ownerUser.Email))
            {
                _logger.LogInformation("RETENTION_POLICY_SENDING_NOTICE: TenantId={TenantId}, Email={Email}", org.Id, ownerUser.Email);

                var (subject, htmlContent) = EmailTemplates.GetRetentionReminderTemplate(org.Name);
                bool isSent = await emailService.SendEmailAsync(ownerUser.Email, subject, htmlContent);

                if (isSent)
                {
                    org.RecordRetentionNoticeSent();
                }
            }
        }

        if (tenantsToNotify.Count > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}