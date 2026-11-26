using CraftFlow.Api.Core.MultiTenancy;
using CraftFlow.Api.Core.Persistence;
using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.Domain;
using CraftFlow.SharedKernel.Enums;
using CraftFlow.SharedKernel.Enums.Subscription;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.Services;

public sealed class TenantEntitlementService : ITenantEntitlementService
{
    private const int UNLIMITED = -1;
    private const int CACHE_MINUTES = 15;

    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly IMemoryCache _cache;

    public TenantEntitlementService(
        AppDbContext dbContext,
        ITenantContext tenantContext,
        IMemoryCache cache)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _cache = cache;
    }

    public async Task<bool> HasOrganizationFeatureAsync(PlanFeatures feature, CancellationToken ct = default)
    {
        if (_tenantContext.IsSuperAdmin) return true;

        var plans = await GetActivePlansAsync(ct);
        return plans.Any(p => (p.Features & feature) == feature);
    }

    public async Task<bool> HasModuleFeatureAsync(AppModule module, PlanFeatures feature, CancellationToken ct = default)
    {
        if (_tenantContext.IsSuperAdmin) return true;
        if (module == AppModule.NONE) return await HasOrganizationFeatureAsync(feature, ct);

        var plans = await GetActivePlansAsync(ct);
        return plans.Any(p => (p.TargetModule & module) == module && (p.Features & feature) == feature);
    }

    public async Task<int> GetAuditRetentionDaysAsync(CancellationToken ct = default)
    {
        if (_tenantContext.IsSuperAdmin) return UNLIMITED;

        var plans = await GetActivePlansAsync(ct);
        return GetMaxLimit(plans, p => p.AuditRetentionDays);
    }

    public async Task<int> GetMaxMonthlyBatchesAsync(AppModule module, CancellationToken ct = default)
    {
        if (_tenantContext.IsSuperAdmin) return UNLIMITED;

        var plans = await GetActivePlansAsync(ct);
        var modulePlans = module == AppModule.NONE
            ? plans
            : plans.Where(p => (p.TargetModule & module) == module).ToList();

        return GetMaxLimit(modulePlans, p => p.MaxMonthlyBatches);
    }

    public async Task<int> GetMaxChambersAsync(AppModule module, CancellationToken ct = default)
    {
        if (_tenantContext.IsSuperAdmin) return UNLIMITED;

        var plans = await GetActivePlansAsync(ct);
        var modulePlans = module == AppModule.NONE
            ? plans
            : plans.Where(p => (p.TargetModule & module) == module).ToList();

        return GetMaxLimit(modulePlans, p => p.MaxChambers);
    }

    public async Task<int> GetMaxWarehousesAsync(CancellationToken ct = default)
    {
        if (_tenantContext.IsSuperAdmin) return UNLIMITED;

        var plans = await GetActivePlansAsync(ct);
        return GetMaxLimit(plans, p => p.MaxWarehouses);
    }

    public async Task<int> GetMaxUsersAsync(CancellationToken ct = default)
    {
        if (_tenantContext.IsSuperAdmin) return UNLIMITED;

        var plans = await GetActivePlansAsync(ct);
        return GetMaxLimit(plans, p => p.MaxUsers);
    }

    public void InvalidateCache(Guid tenantId)
    {
        _cache.Remove(GetCacheKey(tenantId));
    }

    private async Task<List<PlanSnapshot>> GetActivePlansAsync(CancellationToken ct)
    {
        var tenantId = _tenantContext.TenantId;
        if (tenantId == Guid.Empty) return [];

        return await _cache.GetOrCreateAsync(GetCacheKey(tenantId), async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_MINUTES);

            var now = DateTime.UtcNow;

            return await _dbContext.Set<TenantSubscription>()
                .Include(s => s.Plan)
                .AsNoTracking()
                .Where(s => s.TenantId == tenantId &&
                            (s.State == SubscriptionState.Active || s.State == SubscriptionState.Trial) &&
                            (!s.ExpiresAtUtc.HasValue || s.ExpiresAtUtc > now) &&
                            s.Plan != null)
                .Select(s => new PlanSnapshot(
                    s.Plan!.TargetModule,
                    s.Plan.Features,
                    s.Plan.MaxMonthlyBatches,
                    s.Plan.MaxWarehouses,
                    s.Plan.MaxChambers,
                    s.Plan.MaxUsers,
                    s.Plan.AuditRetentionDays))
                .ToListAsync(ct);
        }) ?? [];
    }

    private static int GetMaxLimit(IReadOnlyCollection<PlanSnapshot> plans, Func<PlanSnapshot, int> selector)
    {
        if (plans.Count == 0) return 0;

        var values = plans.Select(selector).ToList();
        return values.Contains(UNLIMITED) ? UNLIMITED : values.Max();
    }

    private static string GetCacheKey(Guid tenantId) => $"entitlements:plans:{tenantId}";

    private sealed record PlanSnapshot(
        AppModule TargetModule,
        PlanFeatures Features,
        int MaxMonthlyBatches,
        int MaxWarehouses,
        int MaxChambers,
        int MaxUsers,
        int AuditRetentionDays);
}