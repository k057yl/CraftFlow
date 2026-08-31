using CraftFlow.Api.Common.Constants;
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

    public SubscriptionQuotaBehavior(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var currentMonthBatchesCount = await _dbContext.Set<ProductionBatch>()
            .CountAsync(b => b.StartedAt >= startOfMonth, cancellationToken);

        if (currentMonthBatchesCount >= CoreConstants.Quotas.MAX_FREE_MONTHLY_BATCHES)
        {
            var error = Error.Validation(ErrorCodes.Saas.QUOTA_EXCEEDED);
            return (TResponse)(object)Result.Failure(error);
        }

        return await next();
    }
}