using CraftFlow.SharedKernel.Enums;

namespace CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.Domain;

public class SubscriptionPlan
{
    public Guid Id { get; private set; }
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;

    public AppModule TargetModule { get; private set; }

    public int MaxMonthlyBatches { get; private set; }
    public int MaxWarehouses { get; private set; }
    public int MaxChambers { get; private set; }
    public int MaxUsers { get; private set; }

    private SubscriptionPlan() { }

    public static SubscriptionPlan Create(
        string code,
        string name,
        AppModule targetModule,
        int maxMonthlyBatches,
        int maxWarehouses,
        int maxChambers,
        int maxUsers)
    {
        return new SubscriptionPlan
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name,
            TargetModule = targetModule,
            MaxMonthlyBatches = maxMonthlyBatches,
            MaxWarehouses = maxWarehouses,
            MaxChambers = maxChambers,
            MaxUsers = maxUsers
        };
    }

    public void UpdateLimits(
        AppModule targetModule,
        int maxMonthlyBatches,
        int maxWarehouses,
        int maxChambers,
        int maxUsers,
        string? name = null)
    {
        TargetModule = targetModule;
        MaxMonthlyBatches = maxMonthlyBatches;
        MaxWarehouses = maxWarehouses;
        MaxChambers = maxChambers;
        MaxUsers = maxUsers;

        if (!string.IsNullOrWhiteSpace(name))
        {
            Name = name;
        }
    }
}