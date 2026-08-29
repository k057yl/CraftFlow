using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Domain;

namespace CraftFlow.Api.Modules.Aging.Domain;

public sealed class AgingLot : AggregateRoot, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid ProductionBatchId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid AgingChamberId { get; private set; }
    public string BatchNumber { get; private set; } = null!;

    public decimal InitialQuantity { get; private set; }
    public decimal CurrentQuantity { get; private set; }
    public AgingStatus Status { get; private set; }

    public DateTime PlacedAt { get; private set; }
    public DateTime TargetReleaseDate { get; private set; }
    public DateTime? ActualReleaseDate { get; private set; }

    private AgingLot() { }

    public static AgingLot Create(
        Guid productionBatchId,
        Guid productId,
        Guid agingChamberId,
        string batchNumber,
        decimal initialQuantity,
        int minAgingDays)
    {
        if (initialQuantity <= 0)
            throw new ArgumentException(ErrorCodes.General.VALUE_REQUIRED);

        var now = DateTime.UtcNow;

        return new AgingLot
        {
            Id = Guid.NewGuid(),
            ProductionBatchId = productionBatchId,
            ProductId = productId,
            AgingChamberId = agingChamberId,
            BatchNumber = batchNumber,
            InitialQuantity = initialQuantity,
            CurrentQuantity = initialQuantity,
            Status = AgingStatus.InChamber,
            PlacedAt = now,
            TargetReleaseDate = now.AddDays(minAgingDays)
        };
    }

    public void RegisterWeightLoss(decimal actualQuantity)
    {
        if (Status != AgingStatus.InChamber)
            throw new InvalidOperationException(ErrorCodes.Aging.INVALID_LOT_STATE);

        if (actualQuantity < 0 || actualQuantity > CurrentQuantity)
            throw new ArgumentException(ErrorCodes.General.VALUE_REQUIRED);

        CurrentQuantity = actualQuantity;
    }

    public void Release()
    {
        if (Status != AgingStatus.InChamber && Status != AgingStatus.ReadyForRelease)
            throw new InvalidOperationException(ErrorCodes.Aging.INVALID_LOT_STATE);

        Status = AgingStatus.Released;
        ActualReleaseDate = DateTime.UtcNow;
    }
}