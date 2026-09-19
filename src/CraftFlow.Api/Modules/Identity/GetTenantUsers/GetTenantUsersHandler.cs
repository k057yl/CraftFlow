using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Identity.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Dtos.Auth;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Identity.GetTenantUsers;
public class GetTenantUsersHandler : IRequestHandler<GetTenantUsersQuery, Result<List<TenantUserDto>>>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public GetTenantUsersHandler(AppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<Result<List<TenantUserDto>>> Handle(GetTenantUsersQuery request, CancellationToken cancellationToken)
    {
        if (_tenantContext.Role != TenantRole.Owner && !_tenantContext.IsSuperAdmin)
        {
            return Result.Failure<List<TenantUserDto>>(Error.Validation(ErrorCodes.Auth.ACCESS_DENIED));
        }

        var currentTenantId = _tenantContext.TenantId;

        var users = await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.TenantId == currentTenantId && u.Role != TenantRole.SuperAdmin)
            .Select(u => new TenantUserDto(u.Id, u.FullName, u.Email, u.Role, u.IsActive))
            .ToListAsync(cancellationToken);

        return Result.Success(users);
    }
}