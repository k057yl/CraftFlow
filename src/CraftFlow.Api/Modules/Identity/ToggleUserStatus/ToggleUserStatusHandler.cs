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

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId && u.TenantId == _tenantContext.TenantId, cancellationToken);

        if (user == null)
        {
            return Result.Failure<bool>(Error.NotFound(ErrorCodes.General.NOT_FOUND));
        }

        if (user.Id == _tenantContext.UserId || user.Role == TenantRole.SuperAdmin)
        {
            return Result.Failure<bool>(Error.Validation(ErrorCodes.Auth.ACCESS_DENIED));
        }

        if (user.IsActive)
        {
            user.Deactivate();
        }
        else
        {
            user.Activate();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(user.IsActive);
    }
}