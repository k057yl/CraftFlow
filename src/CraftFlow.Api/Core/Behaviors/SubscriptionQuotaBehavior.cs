using CraftFlow.Api.Core.MultiTenancy;
using CraftFlow.Api.Core.Persistence;
using CraftFlow.Api.Modules.CoreDomainServices.Identity.Domain;
using CraftFlow.Api.Modules.CoreDomainServices.Inventory.Domain;
using CraftFlow.Api.Modules.CoreDomainServices.Production.Domain;
using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.Domain;
using CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Aging.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Domain;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CraftFlow.Api.Core.Behaviors;

public sealed class SubscriptionQuotaBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private const int UNLIMITED_QUOTA = -1;
    private const int CACHE_EXPIRATION_MINUTES = 15;

    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly IMemoryCache _cache;

    public SubscriptionQuotaBehavior(
        AppDbContext dbContext,
        ITenantContext tenantContext,
        IMemoryCache cache)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _cache = cache;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not IRequireQuotaValidation quotaRequest)
        {
            return await next();
        }

        var tenantId = _tenantContext.TenantId;

        if (tenantId == Guid.Empty || _tenantContext.IsSuperAdmin)
        {
            return await next();
        }

        var subscription = await _dbContext.Set<TenantSubscription>()
            .Include(s => s.Plan)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId, cancellationToken);

        if (subscription == null || !IsSubscriptionValid(subscription))
        {
            return ResultFactory.CreateFailure<TResponse>(Error.Validation(ErrorCodes.Saas.SUBSCRIPTION_EXPIRED));
        }

        var plan = subscription.Plan;
        if (plan == null)
        {
            return ResultFactory.CreateFailure<TResponse>(Error.Validation(ErrorCodes.Saas.SUBSCRIPTION_EXPIRED));
        }

        var isQuotaExceeded = quotaRequest.QuotaType switch
        {
            QuotaType.MONTHLY_BATCHES => await CheckMonthlyBatchesQuotaAsync(tenantId, plan.MaxMonthlyBatches, cancellationToken),
            QuotaType.WAREHOUSES_COUNT => await CheckCountQuotaAsync<Warehouse>(tenantId, plan.MaxWarehouses, cancellationToken),
            QuotaType.CHAMBER_COUNT => await CheckCountQuotaAsync<AgingChamber>(tenantId, plan.MaxChambers, cancellationToken),
            QuotaType.USERS_COUNT => await CheckCountQuotaAsync<OrganizationMember>(tenantId, plan.MaxUsers, cancellationToken),
            _ => false
        };

        if (isQuotaExceeded)
        {
            return ResultFactory.CreateFailure<TResponse>(Error.Validation(ErrorCodes.Saas.QUOTA_EXCEEDED));
        }

        return await next();
    }

    private static bool IsSubscriptionValid(TenantSubscription subscription)
    {
        var isStateActive = subscription.State == SubscriptionState.Active || subscription.State == SubscriptionState.Trial;
        var isNotExpired = !subscription.ExpiresAtUtc.HasValue || subscription.ExpiresAtUtc.Value > DateTime.UtcNow;

        return isStateActive && isNotExpired;
    }

    private async Task<bool> CheckMonthlyBatchesQuotaAsync(Guid tenantId, int maxAllowed, CancellationToken ct)
    {
        if (maxAllowed == UNLIMITED_QUOTA) return false;

        var cacheKey = $"quota:batches:{tenantId}:{DateTime.UtcNow:yyyyMM}";

        var currentCount = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_EXPIRATION_MINUTES);

            var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            return await _dbContext.Set<ProductionBatch>()
                .AsNoTracking()
                .CountAsync(b => b.TenantId == tenantId && b.StartedAt >= startOfMonth, ct);
        });

        return currentCount >= maxAllowed;
    }

    private async Task<bool> CheckCountQuotaAsync<TEntity>(Guid tenantId, int maxAllowed, CancellationToken ct)
        where TEntity : class, ITenantEntity
    {
        if (maxAllowed == UNLIMITED_QUOTA) return false;

        var cacheKey = $"quota:count:{typeof(TEntity).Name}:{tenantId}";

        var currentCount = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_EXPIRATION_MINUTES);

            return await _dbContext.Set<TEntity>()
                .AsNoTracking()
                .CountAsync(e => e.TenantId == tenantId, ct);
        });

        return currentCount >= maxAllowed;
    }
}