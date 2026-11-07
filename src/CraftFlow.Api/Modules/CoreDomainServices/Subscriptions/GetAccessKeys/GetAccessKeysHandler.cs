using CraftFlow.Api.Core.MultiTenancy;
using CraftFlow.Api.Core.Persistence;
using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.Domain;
using CraftFlow.SharedKernel.Dtos.Subscription;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.GetAccessKeys;

public class GetAccessKeysHandler : IRequestHandler<GetAccessKeysQuery, Result<List<AccessKeyDto>>>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public GetAccessKeysHandler(AppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<Result<List<AccessKeyDto>>> Handle(GetAccessKeysQuery request, CancellationToken ct)
    {
        var tenantId = _tenantContext.TenantId;

        var keys = await _dbContext.Set<TenantAccessKey>()
            .AsNoTracking()
            .Where(k => k.TenantId == tenantId)
            .OrderByDescending(k => k.CreatedAtUtc)
            .Select(k => new AccessKeyDto(
                k.Id,
                k.Name,
                $"{k.KeyPrefix}...{k.KeySuffix}",
                k.Status,
                k.CreatedAtUtc,
                k.LastUsedAtUtc
            ))
            .ToListAsync(ct);

        return keys;
    }
}