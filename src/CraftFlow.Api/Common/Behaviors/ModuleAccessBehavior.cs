using System.Collections.Concurrent;
using System.Reflection;
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
    private static readonly ConcurrentDictionary<Type, MethodInfo> FailureMethodCache = new();

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
            return BuildFailureResult(ErrorCodes.Saas.QUOTA_EXCEEDED);
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

    private static TResponse BuildFailureResult(string errorCode)
    {
        var error = Error.Validation(errorCode);

        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(error);
        }

        var genericArgs = typeof(TResponse).GetGenericArguments();
        if (genericArgs.Length == 0)
        {
            return (TResponse)(object)Result.Failure(error);
        }

        var valueType = genericArgs[0];

        var failureMethod = FailureMethodCache.GetOrAdd(valueType, type =>
        {
            var genericMethod = typeof(Result)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m => m.Name == nameof(Result.Failure)
                                     && m.IsGenericMethodDefinition
                                     && m.GetParameters().Length == 1
                                     && m.GetParameters()[0].ParameterType == typeof(Error));

            if (genericMethod != null)
            {
                return genericMethod.MakeGenericMethod(type);
            }

            var genericResultType = typeof(Result<>).MakeGenericType(type);
            return genericResultType
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m => m.Name == nameof(Result.Failure)
                            && m.GetParameters()[0].ParameterType == typeof(Error));
        });

        return (TResponse)failureMethod.Invoke(null, new object[] { error })!;
    }
}