using CraftFlow.Api.Common.Infrastructure.Security;
using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Subscriptions.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace CraftFlow.Api.Modules.Subscriptions.GenerateAccessKey;

public class GenerateAccessKeyHandler : IRequestHandler<GenerateAccessKeyCommand, Result<GenerateAccessKeyResponse>>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly IKeyHasher _keyHasher;

    public GenerateAccessKeyHandler(AppDbContext dbContext, ITenantContext tenantContext, IKeyHasher keyHasher)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _keyHasher = keyHasher;
    }

    public async Task<Result<GenerateAccessKeyResponse>> Handle(GenerateAccessKeyCommand command, CancellationToken ct)
    {
        var tenantId = _tenantContext.TenantId;

        if (string.IsNullOrWhiteSpace(command.KeyName))
        {
            return Result.Failure<GenerateAccessKeyResponse>(Error.Validation("KEY_NAME_REQUIRED"));
        }

        var subscription = await _dbContext.Set<TenantSubscription>()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId, ct);

        if (subscription == null)
        {
            var plan = await _dbContext.Set<SubscriptionPlan>()
                .FirstOrDefaultAsync(p => p.Code == "TRIAL", ct);

            if (plan == null)
            {
                plan = await _dbContext.Set<SubscriptionPlan>().FirstOrDefaultAsync(ct);
            }

            if (plan != null)
            {
                subscription = TenantSubscription.CreateTrial(tenantId, plan.Id, DateTime.UtcNow.AddYears(100));
                subscription.Activate(DateTime.UtcNow.AddYears(100));
                _dbContext.Set<TenantSubscription>().Add(subscription);

                await _dbContext.SaveChangesAsync(ct);
            }
        }

        if (subscription == null)
        {
            return Result.Failure<GenerateAccessKeyResponse>(Error.Validation(ErrorCodes.Saas.SUBSCRIPTION_EXPIRED));
        }

        var randomBytes = RandomNumberGenerator.GetBytes(16);
        var randomHex = Convert.ToHexString(randomBytes).ToLowerInvariant();
        var rawKey = $"cf_live_{randomHex}";

        var prefix = rawKey[..11];
        var suffix = rawKey[^4..];
        var keyHash = _keyHasher.ComputeHash(rawKey);

        var accessKey = TenantAccessKey.Create(
            tenantId,
            subscription.Id,
            command.KeyName,
            prefix,
            suffix,
            keyHash
        );

        _dbContext.Set<TenantAccessKey>().Add(accessKey);
        await _dbContext.SaveChangesAsync(ct);

        return new GenerateAccessKeyResponse(rawKey, accessKey.Name, prefix, suffix);
    }
}