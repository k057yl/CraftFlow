using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Aging.Domain;
using CraftFlow.Api.Modules.Identity.Domain;
using CraftFlow.Api.Modules.Inventory.Domain;
using CraftFlow.Api.Modules.Production.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Domain;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Common.Behaviors;

public sealed class SubscriptionQuotaBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, IRequireQuotaValidation
    where TResponse : Result
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public SubscriptionQuotaBehavior(AppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        var subscription = await _dbContext.TenantSubscriptions
            .Include(s => s.Plan)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId, cancellationToken);

        if (subscription == null || (subscription.ExpiresAtUtc.HasValue && subscription.ExpiresAtUtc < DateTime.UtcNow))
        {
            return BuildFailureResult(ErrorCodes.Saas.SUBSCRIPTION_EXPIRED);
        }

        var plan = subscription.Plan;

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

    private async Task<bool> CheckMonthlyBatchesQuotaAsync(Guid tenantId, int maxAllowed, CancellationToken ct)
    {
        if (maxAllowed == -1) return false;

        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var currentCount = await _dbContext.Set<ProductionBatch>()
            .AsNoTracking()
            .CountAsync(b => b.TenantId == tenantId && b.StartedAt >= startOfMonth, ct);

        return currentCount >= maxAllowed;
    }

    private async Task<bool> CheckCountQuotaAsync<TEntity>(Guid tenantId, int maxAllowed, CancellationToken ct)
        where TEntity : class, ITenantEntity
    {
        if (maxAllowed == -1) return false;

        var currentCount = await _dbContext.Set<TEntity>()
            .AsNoTracking()
            .CountAsync(e => e.TenantId == tenantId, ct);

        return currentCount >= maxAllowed;
    }

    private static TResponse BuildFailureResult(string errorCode)
    {
        var error = Error.Validation(errorCode);

        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(error);
        }

        var resultType = typeof(TResponse).GetGenericArguments()[0];
        var failureMethod = typeof(Result<>)
            .MakeGenericType(resultType)
            .GetMethod(nameof(Result<object>.Failure), new[] { typeof(Error) })!;

        return (TResponse)failureMethod.Invoke(null, new object[] { error })!;
    }
}