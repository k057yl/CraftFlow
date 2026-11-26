using CraftFlow.SharedKernel.Enums;
using CraftFlow.SharedKernel.Enums.Subscription;

namespace CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.Domain;

public class SubscriptionPlan
{
    public Guid Id { get; private set; }
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;

    public AppModule TargetModule { get; private set; }
    public PlanFeatures Features { get; private set; }

    public int MaxMonthlyBatches { get; private set; }
    public int MaxWarehouses { get; private set; }
    public int MaxChambers { get; private set; }
    public int MaxUsers { get; private set; }
    public int AuditRetentionDays { get; private set; }

    private SubscriptionPlan() { }

    public static SubscriptionPlan Create(
        string code,
        string name,
        AppModule targetModule,
        PlanFeatures features,
        int maxMonthlyBatches,
        int maxWarehouses,
        int maxChambers,
        int maxUsers,
        int auditRetentionDays)
    {
        return new SubscriptionPlan
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name,
            TargetModule = targetModule,
            Features = features,
            MaxMonthlyBatches = maxMonthlyBatches,
            MaxWarehouses = maxWarehouses,
            MaxChambers = maxChambers,
            MaxUsers = maxUsers,
            AuditRetentionDays = auditRetentionDays
        };
    }

    public void UpdateLimits(
        AppModule targetModule,
        PlanFeatures features,
        int maxMonthlyBatches,
        int maxWarehouses,
        int maxChambers,
        int maxUsers,
        int auditRetentionDays,
        string? name = null)
    {
        TargetModule = targetModule;
        Features = features;
        MaxMonthlyBatches = maxMonthlyBatches;
        MaxWarehouses = maxWarehouses;
        MaxChambers = maxChambers;
        MaxUsers = maxUsers;
        AuditRetentionDays = auditRetentionDays;

        if (!string.IsNullOrWhiteSpace(name))
        {
            Name = name;
        }
    }
}