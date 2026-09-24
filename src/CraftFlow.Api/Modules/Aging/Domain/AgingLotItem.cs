using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Domain;

namespace CraftFlow.Api.Modules.Aging.Domain;

public enum AgingItemState
{
    InChamber = 1,
    Released = 2,
    Discarded = 3
}

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
            State = AgingItemState.InChamber
        };
    }

    public void RegisterWeightLoss(decimal lossQuantity)
    {
        if (State != AgingItemState.InChamber)
            throw new InvalidOperationException(ErrorCodes.Aging.INVALID_LOT_STATE);

        if (lossQuantity <= 0)
            throw new ArgumentException(ErrorCodes.General.VALUE_REQUIRED);

        CurrentWeight = Math.Max(0.001m, CurrentWeight - lossQuantity);
    }

    public void Release()
    {
        if (State != AgingItemState.InChamber)
            throw new InvalidOperationException(ErrorCodes.Aging.INVALID_LOT_STATE);

        State = AgingItemState.Released;
    }

    public void Discard(string reason)
    {
        if (State == AgingItemState.Discarded)
            return;

        State = AgingItemState.Discarded;
        DiscardReason = reason;
        CurrentWeight = 0;
    }
}