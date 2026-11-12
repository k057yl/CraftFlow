using CraftFlow.Api.Core.Persistence;
using CraftFlow.SharedKernel.Dtos.Subscription;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.GetPlans;

public class GetSubscriptionPlansHandler : IRequestHandler<GetSubscriptionPlansQuery, Result<List<SubscriptionPlanDto>>>
{
    private readonly AppDbContext _dbContext;

    public GetSubscriptionPlansHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<SubscriptionPlanDto>>> Handle(GetSubscriptionPlansQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.SubscriptionPlans.AsNoTracking();

        if (request.OrganizationId.HasValue && request.OrganizationId.Value != Guid.Empty)
        {
            var orgModules = await _dbContext.TenantSubscriptions
                .IgnoreQueryFilters()
                .Include(s => s.Plan)
                .Where(s => s.TenantId == request.OrganizationId.Value)
                .Select(s => s.Plan.TargetModule)
                .Distinct()
                .ToListAsync(cancellationToken);

            query = orgModules.Count > 0
                ? query.Where(p => orgModules.Contains(p.TargetModule))
                : query.Where(p => p.Code.EndsWith("_FREE"));
        }

        var plans = await query
            .OrderBy(p => p.TargetModule)
            .ThenBy(p => p.Code)
            .Select(p => new SubscriptionPlanDto(p.Id, p.Code, p.Name))
            .ToListAsync(cancellationToken);

        return Result.Success(plans);
    }
}