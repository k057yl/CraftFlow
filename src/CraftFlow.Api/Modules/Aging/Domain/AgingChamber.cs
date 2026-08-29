using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Domain;

namespace CraftFlow.Api.Modules.Aging.Domain;

public sealed class AgingChamber : AggregateRoot, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = null!;
    public decimal TargetTemperature { get; private set; }
    public decimal TargetHumidity { get; private set; }

    private AgingChamber() { }

    public static AgingChamber Create(string name, decimal targetTemperature, decimal targetHumidity)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(ErrorCodes.Aging.CHAMBER_NAME_REQUIRED);

        return new AgingChamber
        {
            Id = Guid.NewGuid(),
            Name = name,
            TargetTemperature = targetTemperature,
            TargetHumidity = targetHumidity
        };
    }
}