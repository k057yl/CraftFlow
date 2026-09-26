using System.Collections.Concurrent;
using System.Reflection;
using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Aging.Domain;
using CraftFlow.Api.Modules.Identity.Domain;
using CraftFlow.Api.Modules.Inventory.Domain;
using CraftFlow.Api.Modules.Production.Domain;
using CraftFlow.Api.Modules.Subscriptions.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Domain;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CraftFlow.Api.Common.Behaviors;

public sealed class SubscriptionQuotaBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, IRequireQuotaValidation
    where TResponse : Result
{
    private const int UNLIMITED_QUOTA = -1;
    private const int CACHE_EXPIRATION_MINUTES = 15;

    private static readonly ConcurrentDictionary<Type, MethodInfo> FailureMethodCache = new();

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
            return BuildFailureResult(ErrorCodes.Saas.SUBSCRIPTION_EXPIRED);
        }

        var plan = subscription.Plan;
        if (plan == null)
        {
            return BuildFailureResult(ErrorCodes.Saas.SUBSCRIPTION_EXPIRED);
        }

        var isQuotaExceeded = request.QuotaType switch
        {
            QuotaType.MonthlyBatches => await CheckMonthlyBatchesQuotaAsync(tenantId, plan.MaxMonthlyBatches, cancellationToken),
            QuotaType.WarehousesCount => await CheckCountQuotaAsync<Warehouse>(tenantId, plan.MaxWarehouses, cancellationToken),
            QuotaType.ChamberCount => await CheckCountQuotaAsync<AgingChamber>(tenantId, plan.MaxChambers, cancellationToken),
            QuotaType.UsersCount => await CheckCountQuotaAsync<User>(tenantId, plan.MaxUsers, cancellationToken),
            _ => false
        };

        if (isQuotaExceeded)
        {
            return BuildFailureResult(ErrorCodes.Saas.QUOTA_EXCEEDED);
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

    private static TResponse BuildFailureResult(string errorCode)
    {
        var error = Error.Validation(errorCode);

        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(error);
        }

        var genericArgs = typeof(TResponse).GetGenericArguments();
        if (genericArgs.Length == 0)
        {
            return (TResponse)(object)Result.Failure(error);
        }

        var valueType = genericArgs[0];

        var failureMethod = FailureMethodCache.GetOrAdd(valueType, type =>
        {
            var genericMethod = typeof(Result)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m => m.Name == nameof(Result.Failure)
                                  && m.IsGenericMethodDefinition
                                  && m.GetParameters().Length == 1
                                  && m.GetParameters()[0].ParameterType == typeof(Error));

            if (genericMethod != null)
            {
                return genericMethod.MakeGenericMethod(type);
            }

            var genericResultType = typeof(Result<>).MakeGenericType(type);
            return genericResultType
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m => m.Name == nameof(Result.Failure)
                         && m.GetParameters()[0].ParameterType == typeof(Error));
        });

        return (TResponse)failureMethod.Invoke(null, new object[] { error })!;
    }
}