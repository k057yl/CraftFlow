using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Domain;
using CraftFlow.SharedKernel.Enums.Inventory;

namespace CraftFlow.Api.Modules.Aging.Domain;

public class AgingLotItem : Entity
{
    public Guid AgingLotId { get; private set; }
    public string ItemNumber { get; private set; } = null!;
    public decimal InitialWeight { get; private set; }
    public decimal CurrentWeight { get; private set; }
    public AgingItemState State { get; private set; }
    public string? DiscardReason { get; private set; }

    private AgingLotItem() { }

    public static AgingLotItem Create(Guid agingLotId, string itemNumber, decimal weight)
    {
        if (weight <= 0)
            throw new ArgumentException(ErrorCodes.General.VALUE_REQUIRED);

        return new AgingLotItem
        {
            Id = Guid.NewGuid(),
            AgingLotId = agingLotId,
            ItemNumber = itemNumber,
            InitialWeight = weight,
            CurrentWeight = weight,
            State = AgingItemState.IN_CHAMBER
        };
    }

    public void RegisterWeightLoss(decimal lossQuantity)
    {
        if (State != AgingItemState.IN_CHAMBER)
            throw new InvalidOperationException(ErrorCodes.Aging.INVALID_LOT_STATE);

        if (lossQuantity <= 0)
            throw new ArgumentException(ErrorCodes.General.VALUE_REQUIRED);

        CurrentWeight = Math.Max(0.001m, CurrentWeight - lossQuantity);
    }

    public void Release()
    {
        if (State != AgingItemState.IN_CHAMBER)
            throw new InvalidOperationException(ErrorCodes.Aging.INVALID_LOT_STATE);

        State = AgingItemState.RELEASED;
    }

    public void Discard(string reason)
    {
        if (State == AgingItemState.DISCARDED)
            return;

        State = AgingItemState.DISCARDED;
        DiscardReason = reason;
        CurrentWeight = 0;
    }

    public void SetFinalWeight(decimal weight)
    {
        if (State != AgingItemState.IN_CHAMBER)
            throw new InvalidOperationException(ErrorCodes.Aging.INVALID_LOT_STATE);

        if (weight <= 0)
            throw new ArgumentException(ErrorCodes.General.VALUE_REQUIRED);

        CurrentWeight = weight;
    }
}