using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Identity.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Identity.CreateTenantUser;

public class CreateTenantUserHandler : IRequestHandler<CreateTenantUserCommand, Result<Guid>>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public CreateTenantUserHandler(AppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(CreateTenantUserCommand request, CancellationToken cancellationToken)
    {
        if (_tenantContext.Role != TenantRole.Owner && !_tenantContext.IsSuperAdmin)
        {
            return Result.Failure<Guid>(Error.Validation(ErrorCodes.Auth.ACCESS_DENIED));
        }

        var tenantId = _tenantContext.TenantId;
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (user == null)
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            user = User.Create(normalizedEmail, passwordHash, request.FullName);
            user.Activate();
            _dbContext.Users.Add(user);
        }

        var organization = await _dbContext.Organizations
            .IgnoreQueryFilters()
            .Include(o => o.Members)
            .FirstOrDefaultAsync(o => o.Id == tenantId, cancellationToken);

        if (organization == null)
        {
            return Result.Failure<Guid>(Error.NotFound("ORGANIZATION_NOT_FOUND"));
        }

        var member = organization.AddMember(user.Id, request.Role);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(member.Id);
    }
}