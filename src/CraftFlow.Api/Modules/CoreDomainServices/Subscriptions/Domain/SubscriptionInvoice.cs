using CraftFlow.SharedKernel.Domain;
using CraftFlow.SharedKernel.Enums.Subscription;

namespace CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.Domain;
public class SubscriptionInvoice : Entity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public string PlanCode { get; private set; } = null!;
    public int DaysRequested { get; private set; }
    public decimal Amount { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ProcessedAtUtc { get; private set; }

    private SubscriptionInvoice() { }

    public static SubscriptionInvoice Create(Guid tenantId, string planCode, int daysRequested, decimal amount = 0m)
    {
        return new SubscriptionInvoice
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PlanCode = planCode.ToUpperInvariant(),
            DaysRequested = daysRequested,
            Amount = amount,
            Status = InvoiceStatus.PENDING,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void Approve()
    {
        Status = InvoiceStatus.APPROVED;
        ProcessedAtUtc = DateTime.UtcNow;
    }

    public void Reject()
    {
        Status = InvoiceStatus.REJECTED;
        ProcessedAtUtc = DateTime.UtcNow;
    }
}