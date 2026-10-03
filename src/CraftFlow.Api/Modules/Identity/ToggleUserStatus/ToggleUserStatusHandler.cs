using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Identity.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Identity.ToggleUserStatus;

public class ToggleUserStatusHandler : IRequestHandler<ToggleUserStatusCommand, Result<bool>>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public ToggleUserStatusHandler(AppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<Result<bool>> Handle(ToggleUserStatusCommand request, CancellationToken cancellationToken)
    {
        if (_tenantContext.Role != TenantRole.Owner && !_tenantContext.IsSuperAdmin)
        {
            return Result.Failure<bool>(Error.Validation(ErrorCodes.Auth.ACCESS_DENIED));
        }

        var member = await _dbContext.OrganizationMembers
            .FirstOrDefaultAsync(m => m.UserId == request.UserId && m.TenantId == _tenantContext.TenantId, cancellationToken);

        if (member == null)
        {
            return Result.Failure<bool>(Error.NotFound(ErrorCodes.General.NOT_FOUND));
        }

        if (member.UserId == _tenantContext.UserId || member.Role == TenantRole.SuperAdmin)
        {
            return Result.Failure<bool>(Error.Validation(ErrorCodes.Auth.ACCESS_DENIED));
        }

        if (member.IsActive)
        {
            member.Deactivate();
        }
        else
        {
            member.Activate();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(member.IsActive);
    }
}