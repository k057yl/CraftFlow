using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Subscriptions.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CraftFlow.Api.Modules.Subscriptions.RevokeAccessKey;
public class RevokeAccessKeyHandler : IRequestHandler<RevokeAccessKeyCommand, Result>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly IMemoryCache _cache;

    public RevokeAccessKeyHandler(AppDbContext dbContext, ITenantContext tenantContext, IMemoryCache cache)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _cache = cache;
    }

    public async Task<Result> Handle(RevokeAccessKeyCommand command, CancellationToken ct)
    {
        var tenantId = _tenantContext.TenantId;

        var accessKey = await _dbContext.Set<TenantAccessKey>()
            .FirstOrDefaultAsync(k => k.Id == command.KeyId && k.TenantId == tenantId, ct);

        if (accessKey == null)
        {
            return Result.Failure(Error.NotFound(ErrorCodes.General.NOT_FOUND));
        }

        if (accessKey.Status == KeyStatus.Revoked)
        {
            return Result.Success();
        }

        accessKey.Revoke(DateTime.UtcNow);

        await _dbContext.SaveChangesAsync(ct);

        var cacheKey = $"{AuthConstants.Cache.ACCESS_KEY_PREFIX}{accessKey.KeyHash}";
        _cache.Remove(cacheKey);

        return Result.Success();
    }
}