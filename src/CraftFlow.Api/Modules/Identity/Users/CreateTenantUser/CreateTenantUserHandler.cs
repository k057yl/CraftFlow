using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Identity.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CraftFlow.Api.Modules.Identity.Users.CreateTenantUser;

public class CreateTenantUserHandler : IRequestHandler<CreateTenantUserCommand, Result<Guid>>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly IMemoryCache _cache;

    public CreateTenantUserHandler(
        AppDbContext dbContext,
        ITenantContext tenantContext,
        IMemoryCache cache)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _cache = cache;
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
            return Result.Failure<Guid>(Error.NotFound(ErrorCodes.General.NOT_FOUND));
        }

        var member = organization.AddMember(user.Id, request.Role);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var cacheKey = $"quota:count:{nameof(OrganizationMember)}:{_tenantContext.TenantId}";
        _cache.Remove(cacheKey);

        return Result.Success(member.Id);
    }
}