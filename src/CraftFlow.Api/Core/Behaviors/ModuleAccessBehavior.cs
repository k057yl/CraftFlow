using CraftFlow.Api.Core.MultiTenancy;
using CraftFlow.Api.Core.Persistence;
using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Enums;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CraftFlow.Api.Core.Behaviors;

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

        var activeModules = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CACHE_EXPIRATION_MINUTES);

            var activeSubscriptions = await _dbContext.Set<TenantSubscription>()
                .Include(s => s.Plan)
                .AsNoTracking()
                .Where(s => s.TenantId == tenantId &&
                            (s.State == SubscriptionState.Active || s.State == SubscriptionState.Trial) &&
                            (!s.ExpiresAtUtc.HasValue || s.ExpiresAtUtc > DateTime.UtcNow))
                .ToListAsync(ct);

            var totalAccess = AppModule.NONE;
            foreach (var sub in activeSubscriptions)
            {
                if (sub.Plan != null)
                {
                    totalAccess |= sub.Plan.TargetModule;
                }
            }

            return totalAccess;
        });

        return (activeModules & requiredModule) == requiredModule;
    }
}