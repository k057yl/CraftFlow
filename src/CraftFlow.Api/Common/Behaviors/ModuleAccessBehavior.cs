using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Subscriptions.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Enums.Saas;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CraftFlow.Api.Common.Behaviors;

public sealed class ModuleAccessBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private const int CACHE_EXPIRATION_MINUTES = 15;

    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly IMemoryCache _cache;

    public ModuleAccessBehavior(
        AppDbContext dbContext,
        ITenantContext tenantContext,
        IMemoryCache cache)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _cache = cache;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not IRequireModule moduleRequest)
        {
            return await next();
        }

        var tenantId = _tenantContext.TenantId;

        if (tenantId == Guid.Empty || _tenantContext.IsSuperAdmin)
        {
            return await next();
        }

        var hasAccess = await CheckModuleAccessAsync(tenantId, moduleRequest.RequiredModule, cancellationToken);

        if (!hasAccess)
        {
            return ResultFactory.CreateFailure<TResponse>(Error.Forbidden(ErrorCodes.Saas.MODULE_NOT_PURCHASED));
        }

        return await next();
    }

    private async Task<bool> CheckModuleAccessAsync(Guid tenantId, AppModule requiredModule, CancellationToken ct)
    {
        var cacheKey = $"quota:modules:{tenantId}";

        var enabledModules = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_EXPIRATION_MINUTES);

            var subscription = await _dbContext.Set<TenantSubscription>()
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.TenantId == tenantId, ct);

            return subscription?.EnabledModules ?? AppModule.NONE;
        });

        return (enabledModules & requiredModule) == requiredModule;
    }
}