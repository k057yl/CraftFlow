using Microsoft.Extensions.Caching.Memory;

namespace CraftFlow.Api.Common.Behaviors;
public static class QuotaCacheExtensions
{
    public static void InvalidateQuotaCache<TEntity>(this IMemoryCache cache, Guid tenantId)
    {
        var cacheKey = $"quota:count:{typeof(TEntity).Name}:{tenantId}";
        cache.Remove(cacheKey);
    }
}