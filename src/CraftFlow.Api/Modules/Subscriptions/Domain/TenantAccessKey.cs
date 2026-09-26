using CraftFlow.SharedKernel.Domain;

namespace CraftFlow.Api.Modules.Subscriptions.Domain;

public class TenantAccessKey : Entity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid SubscriptionId { get; private set; }

    public string Name { get; private set; } = null!;
    public string KeyPrefix { get; private set; } = null!;
    public string KeySuffix { get; private set; } = null!;
    public string KeyHash { get; private set; } = null!;

    public KeyStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public DateTime? LastUsedAtUtc { get; private set; }

    public TenantSubscription Subscription { get; private set; } = null!;

    private TenantAccessKey() { }

    public static TenantAccessKey Create(
        Guid tenantId,
        Guid subscriptionId,
        string name,
        string keyPrefix,
        string keySuffix,
        string keyHash)
    {
        return new TenantAccessKey
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SubscriptionId = subscriptionId,
            Name = name.Trim(),
            KeyPrefix = keyPrefix,
            KeySuffix = keySuffix,
            KeyHash = keyHash,
            Status = KeyStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void UpdateLastUsed(DateTime usedAtUtc)
    {
        LastUsedAtUtc = usedAtUtc;
    }

    public void Revoke(DateTime revokedAtUtc)
    {
        Status = KeyStatus.Revoked;
        RevokedAtUtc = revokedAtUtc;
    }
}