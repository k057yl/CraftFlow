using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Identity.ToggleOrganizationStatus;

public class ToggleOrganizationStatusHandler : IRequestHandler<ToggleOrganizationStatusCommand, Result<bool>>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public ToggleOrganizationStatusHandler(AppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<Result<bool>> Handle(ToggleOrganizationStatusCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.IsAdmin)
        {
            return Result.Failure<bool>(Error.Validation(ErrorCodes.Auth.INVALID_CREDENTIALS));
        }

        var organization = await _dbContext.Organizations
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(o => o.Id == request.OrganizationId, cancellationToken);

        if (organization == null)
        {
            return Result.Failure<bool>(Error.NotFound(ErrorCodes.Auth.USER_NOT_FOUND));
        }

        if (organization.IsActive)
        {
            organization.Deactivate();
        }
        else
        {
            organization.Activate();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(true);
    }
}