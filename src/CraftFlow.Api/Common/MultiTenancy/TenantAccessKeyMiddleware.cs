using CraftFlow.Api.Common.Infrastructure.Security;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Subscriptions.Domain;
using CraftFlow.SharedKernel.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CraftFlow.Api.Common.MultiTenancy;

public class TenantAccessKeyMiddleware
{
    private readonly RequestDelegate _next;

    public TenantAccessKeyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IKeyHasher keyHasher,
        IMemoryCache cache,
        AppDbContext dbContext)
    {
        if (!context.Request.Headers.TryGetValue(AuthConstants.Headers.SUBSCRIPTION_KEY, out var rawKey)
            || string.IsNullOrWhiteSpace(rawKey))
        {
            await _next(context);
            return;
        }

        var keyHash = keyHasher.ComputeHash(rawKey.ToString());
        var cacheKey = $"{AuthConstants.Cache.ACCESS_KEY_PREFIX}{keyHash}";

        if (!cache.TryGetValue(cacheKey, out TenantKeyValidationResult? validationResult) || validationResult == null)
        {
            var accessKey = await dbContext.Set<TenantAccessKey>()
                .Include(k => k.Subscription)
                .AsNoTracking()
                .FirstOrDefaultAsync(k => k.KeyHash == keyHash);

            if (accessKey == null || accessKey.Status != KeyStatus.Active)
            {
                validationResult = TenantKeyValidationResult.Failure(ErrorCodes.Auth.INVALID_ACCESS_KEY);
            }
            else if (accessKey.Subscription.State != SubscriptionState.Active && accessKey.Subscription.State != SubscriptionState.Trial)
            {
                validationResult = TenantKeyValidationResult.Failure(ErrorCodes.Saas.SUBSCRIPTION_EXPIRED);
            }
            else
            {
                validationResult = TenantKeyValidationResult.Success(accessKey.TenantId, accessKey.Id);

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));

                cache.Set(cacheKey, validationResult, cacheOptions);
            }
        }

        if (!validationResult.IsValid)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync(validationResult.ErrorCode!);
            return;
        }

        context.Items["TenantId"] = validationResult.TenantId;

        var lastUsedCacheKey = $"key_last_used_{validationResult.AccessKeyId}";
        if (!cache.TryGetValue(lastUsedCacheKey, out _))
        {
            var keyId = validationResult.AccessKeyId;
            _ = Task.Run(async () =>
            {
                using var scope = context.RequestServices.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                await db.Set<TenantAccessKey>()
                    .Where(k => k.Id == keyId)
                    .ExecuteUpdateAsync(s => s.SetProperty(k => k.LastUsedAtUtc, DateTime.UtcNow));
            });

            cache.Set(lastUsedCacheKey, true, TimeSpan.FromHours(2));
        }

        await _next(context);
    }
}

public record TenantKeyValidationResult
{
    public bool IsValid { get; init; }
    public Guid TenantId { get; init; }
    public Guid AccessKeyId { get; init; }
    public string? ErrorCode { get; init; }

    public static TenantKeyValidationResult Success(Guid tenantId, Guid accessKeyId) =>
        new() { IsValid = true, TenantId = tenantId, AccessKeyId = accessKeyId };

    public static TenantKeyValidationResult Failure(string errorCode) =>
        new() { IsValid = false, ErrorCode = errorCode };
}