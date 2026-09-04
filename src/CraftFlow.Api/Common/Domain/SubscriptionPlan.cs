using CraftFlow.SharedKernel.Domain;

namespace CraftFlow.Api.Common.Domain;

public class SubscriptionPlan : Entity
{
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;

    public int MaxMonthlyBatches { get; private set; }
    public int MaxWarehouses { get; private set; }
    public int MaxChambers { get; private set; }
    public int MaxUsers { get; private set; }

    private SubscriptionPlan() { }

    public SubscriptionPlan(
        Guid id,
        string code,
        string name,
        int maxMonthlyBatches,
        int maxWarehouses,
        int maxChambers,
        int maxUsers) : base(id)
    {
        Code = code;
        Name = name;
        MaxMonthlyBatches = maxMonthlyBatches;
        MaxWarehouses = maxWarehouses;
        MaxChambers = maxChambers;
        MaxUsers = maxUsers;
    }
}