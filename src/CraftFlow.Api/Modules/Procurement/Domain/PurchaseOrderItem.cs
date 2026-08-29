using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Domain;

namespace CraftFlow.Api.Modules.Procurement.Domain;

public sealed class PurchaseOrderItem : Entity
{
    public Guid PurchaseOrderId { get; private set; }
    public Guid RawMaterialId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    private PurchaseOrderItem() { }

    internal static PurchaseOrderItem Create(Guid rawMaterialId, decimal quantity, decimal unitPrice)
    {
        if (quantity <= 0)
            throw new ArgumentException(ErrorCodes.Procurement.PURCHASE_ORDER_ITEM_INVALID_QUANTITY);

        if (unitPrice < 0)
            throw new ArgumentException(ErrorCodes.Procurement.PURCHASE_ORDER_ITEM_INVALID_PRICE);

        return new PurchaseOrderItem
        {
            Id = Guid.NewGuid(),
            RawMaterialId = rawMaterialId,
            Quantity = quantity,
            UnitPrice = unitPrice
        };
    }
}
