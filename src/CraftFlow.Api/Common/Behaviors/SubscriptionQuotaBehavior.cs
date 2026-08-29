using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Production.Domain;
using CraftFlow.SharedKernel.Constants;
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
        const int MAX_MONTHLY_BATCHES_FREE_LIMIT = 4;

        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var currentMonthBatchesCount = await _dbContext.Set<ProductionBatch>()
            .CountAsync(b => b.StartedAt >= startOfMonth, cancellationToken);

        if (currentMonthBatchesCount >= MAX_MONTHLY_BATCHES_FREE_LIMIT)
        {
            var error = Error.Validation(ErrorCodes.Saas.QUOTA_EXCEEDED);
            return (TResponse)(object)Result.Failure(error);
        }

        return await next();
    }
}