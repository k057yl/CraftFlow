using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Domain;

namespace CraftFlow.Api.Modules.Sales.Domain;

public sealed class SalesOrderItem : Entity
{
    public Guid SalesOrderId { get; private set; }
    public Guid StockLotId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    private SalesOrderItem() { }

    public static SalesOrderItem Create(Guid stockLotId, decimal quantity, decimal unitPrice)
    {
        if (stockLotId == Guid.Empty || quantity <= 0 || unitPrice < 0)
            throw new ArgumentException(ErrorCodes.General.VALUE_REQUIRED);

        return new SalesOrderItem
        {
            Id = Guid.NewGuid(),
            StockLotId = stockLotId,
            Quantity = quantity,
            UnitPrice = unitPrice
        };
    }
}