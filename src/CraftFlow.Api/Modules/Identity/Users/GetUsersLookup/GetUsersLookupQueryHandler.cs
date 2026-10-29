using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Dtos.Common;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Identity.Users.GetUsersLookup;

public sealed class GetUsersLookupQueryHandler : IRequestHandler<GetUsersLookupQuery, Result<List<LookupDto>>>
{
    private readonly AppDbContext _context;
    private readonly ITenantContext _tenantContext;

    public GetUsersLookupQueryHandler(AppDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<Result<List<LookupDto>>> Handle(GetUsersLookupQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.IsResolved || _tenantContext.TenantId == Guid.Empty)
            return Result.Failure<List<LookupDto>>(ErrorCodes.Auth.UNAUTHORIZED);

        var allowedRoles = new[]
        {
            TenantRole.OWNER,
            TenantRole.TECHNOLOGIST,
            TenantRole.OPERATOR,
            TenantRole.STOREKEEPER
        };

        var users = await _context.OrganizationMembers
            .AsNoTracking()
            .Where(m => m.TenantId == _tenantContext.TenantId &&
                        m.Status == MemberStatus.ACTIVE &&
                        allowedRoles.Contains(m.Role) &&
                        m.User.Status == UserStatus.ACTIVE)
            .Select(m => new LookupDto(m.User.Id, m.User.FullName, null))
            .ToListAsync(cancellationToken);

        return Result.Success(users);
    }
}