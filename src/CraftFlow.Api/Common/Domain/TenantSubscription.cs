namespace CraftFlow.Api.Common.Domain;

public class TenantSubscription
{
    public Guid TenantId { get; set; }
    public Guid PlanId { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }

    public SubscriptionPlan Plan { get; set; } = null!;
}