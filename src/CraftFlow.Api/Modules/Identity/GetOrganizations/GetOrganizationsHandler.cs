using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Identity.GetOrganizations;

public class GetOrganizationsHandler : IRequestHandler<GetOrganizationsQuery, Result<List<OrganizationAdminDto>>>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public GetOrganizationsHandler(AppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<Result<List<OrganizationAdminDto>>> Handle(GetOrganizationsQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.IsAdmin)
        {
            return Result.Failure<List<OrganizationAdminDto>>(Error.Validation(ErrorCodes.Auth.INVALID_CREDENTIALS));
        }

        var orgs = await _dbContext.Organizations
            .IgnoreQueryFilters()
            .Where(o => o.IsActive)
            .AsNoTracking()
            .Select(o => new
            {
                Organization = o,
                SubscriptionData = _dbContext.TenantSubscriptions
                    .IgnoreQueryFilters()
                    .Where(s => s.TenantId == o.Id)
                    .OrderByDescending(s => s.ExpiresAtUtc)
                    .Select(s => new
                    {
                        Subscription = s,
                        PlanCode = s.Plan.Code
                    })
                    .FirstOrDefault()
            })
            .Select(x => new OrganizationAdminDto(
                x.Organization.Id,
                x.Organization.Name,
                x.Organization.IsActive,
                x.Organization.CreatedAtUtc,
                x.SubscriptionData != null && x.SubscriptionData.PlanCode != null ? x.SubscriptionData.PlanCode : "FREE",
                x.SubscriptionData != null ? x.SubscriptionData.Subscription.ExpiresAtUtc : null
            ))
            .ToListAsync(cancellationToken);

        return Result.Success(orgs);
    }
}