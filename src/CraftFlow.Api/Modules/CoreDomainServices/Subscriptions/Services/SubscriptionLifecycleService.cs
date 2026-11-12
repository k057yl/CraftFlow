using CraftFlow.Api.Core.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.Services;

public class SubscriptionLifecycleService
{
    private readonly AppDbContext _dbContext;

    public SubscriptionLifecycleService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> DowngradeExpiredModulesAsync(Guid? tenantId = null, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var expiredCandidateModules = await _dbContext.TenantSubscriptions
            .IgnoreQueryFilters()
            .Include(s => s.Plan)
            .Where(s => (!tenantId.HasValue || s.TenantId == tenantId.Value)
                        && s.ExpiresAtUtc.HasValue
                        && s.ExpiresAtUtc.Value <= now)
            .Select(s => new { s.TenantId, s.Plan.TargetModule })
            .Distinct()
            .ToListAsync(ct);

        if (expiredCandidateModules.Count == 0)
            return false;

        var freePlans = await _dbContext.SubscriptionPlans
            .AsNoTracking()
            .Where(p => p.Code.EndsWith("_FREE"))
            .ToDictionaryAsync(p => p.TargetModule, p => p, ct);

        var targetTenantIds = expiredCandidateModules.Select(x => x.TenantId).Distinct().ToList();

        var allTenantSubs = await _dbContext.TenantSubscriptions
            .IgnoreQueryFilters()
            .Include(s => s.Plan)
            .Where(s => targetTenantIds.Contains(s.TenantId))
            .ToListAsync(ct);

        bool hasChanges = false;

        foreach (var item in expiredCandidateModules)
        {
            if (!freePlans.TryGetValue(item.TargetModule, out var freePlan))
                continue;

            var moduleSubs = allTenantSubs
                .Where(s => s.TenantId == item.TenantId && s.Plan.TargetModule == item.TargetModule)
                .ToList();

            bool hasActiveSub = moduleSubs.Any(s => !s.ExpiresAtUtc.HasValue || s.ExpiresAtUtc.Value > now);
            if (hasActiveSub)
                continue;

            var latestSub = moduleSubs
                .OrderByDescending(s => s.ExpiresAtUtc)
                .First();

            latestSub.DowngradeToFree(freePlan.Id);

            var duplicates = moduleSubs.Where(s => s.Id != latestSub.Id).ToList();
            if (duplicates.Count > 0)
            {
                _dbContext.TenantSubscriptions.RemoveRange(duplicates);
            }

            hasChanges = true;
        }

        if (hasChanges)
        {
            await _dbContext.SaveChangesAsync(ct);
        }

        return hasChanges;
    }
}