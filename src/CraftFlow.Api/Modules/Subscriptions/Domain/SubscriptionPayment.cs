namespace CraftFlow.Api.Modules.Subscriptions.Domain;

public class SubscriptionPayment
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public decimal Amount { get; private set; }
    public string PlanCode { get; private set; } = null!;
    public int DaysAdded { get; private set; }
    public string Description { get; private set; } = null!;
    public DateTime CreatedAtUtc { get; private set; }

    private SubscriptionPayment() { }

    public static SubscriptionPayment Create(
        Guid tenantId,
        decimal amount,
        string planCode,
        int daysAdded,
        string description)
    {
        return new SubscriptionPayment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Amount = amount,
            PlanCode = planCode,
            DaysAdded = daysAdded,
            Description = description,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}