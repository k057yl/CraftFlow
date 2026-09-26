using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Domain;

namespace CraftFlow.Api.Modules.Procurement.Domain;

public sealed class PurchaseOrder : AggregateRoot, ITenantEntity
{
    private readonly List<PurchaseOrderItem> _items = [];

    public Guid TenantId { get; private set; }
    public Guid SupplierId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public PurchaseOrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<PurchaseOrderItem> Items => _items.AsReadOnly();

    private PurchaseOrder() { }

    public static PurchaseOrder Create(Guid tenantId, Guid supplierId, Guid warehouseId)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException(ErrorCodes.General.VALUE_REQUIRED);

        return new PurchaseOrder
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SupplierId = supplierId,
            WarehouseId = warehouseId,
            Status = PurchaseOrderStatus.Draft,
            TotalAmount = 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void AddItem(Guid rawMaterialId, decimal quantity, decimal unitPrice)
    {
        if (Status != PurchaseOrderStatus.Draft)
            throw new InvalidOperationException(ErrorCodes.Procurement.PURCHASE_ORDER_INVALID_STATUS);

        var item = PurchaseOrderItem.Create(rawMaterialId, quantity, unitPrice);
        _items.Add(item);
        TotalAmount += quantity * unitPrice;
    }

    public void Submit()
    {
        if (Status != PurchaseOrderStatus.Draft)
            throw new InvalidOperationException(ErrorCodes.Procurement.PURCHASE_ORDER_INVALID_STATUS);

        Status = PurchaseOrderStatus.Submitted;
    }

    public void MarkAsReceived()
    {
        if (Status != PurchaseOrderStatus.Submitted)
            throw new InvalidOperationException(ErrorCodes.Procurement.PURCHASE_ORDER_INVALID_STATUS);

        Status = PurchaseOrderStatus.Received;
    }
}
