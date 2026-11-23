using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Domain;

namespace CraftFlow.Api.Modules.CoreDomainServices.Inventory.Domain;

public sealed class StockLotItem : Entity
{
    public Guid StockLotId { get; private set; }
    public string ItemNumber { get; private set; } = null!;
    public decimal CurrentWeight { get; private set; }
    public bool IsSold { get; private set; }

    private StockLotItem() { }

    public static StockLotItem Create(Guid stockLotId, string itemNumber, decimal weight)
    {
        if (weight <= 0)
            throw new ArgumentException(ErrorCodes.General.VALUE_REQUIRED);

        return new StockLotItem
        {
            Id = Guid.NewGuid(),
            StockLotId = stockLotId,
            ItemNumber = itemNumber,
            CurrentWeight = weight,
            IsSold = false
        };
    }

    public void MarkAsSold()
    {
        if (IsSold)
            throw new InvalidOperationException("ITEM_ALREADY_SOLD");

        IsSold = true;
    }
}