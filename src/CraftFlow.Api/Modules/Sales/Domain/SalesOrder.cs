using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Domain;

namespace CraftFlow.Api.Modules.Sales.Domain
{
    public sealed class SalesOrder : AggregateRoot, ITenantEntity
    {
        private readonly List<SalesOrderItem> _items = [];

        public Guid TenantId { get; private set; }
        public Guid CustomerId { get; private set; }
        public Guid WarehouseId { get; private set; }
        public OrderStatus Status { get; private set; }
        public decimal TotalAmount { get; private set; }
        public IReadOnlyCollection<SalesOrderItem> Items => _items.AsReadOnly();

        private SalesOrder() { }

        public static SalesOrder Create(Guid customerId, Guid warehouseId)
        {
            return new SalesOrder
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                WarehouseId = warehouseId,
                Status = OrderStatus.Draft,
                TotalAmount = 0
            };
        }

        public void AddItem(Guid productId, decimal quantity, decimal unitPrice)
        {
            if (Status != OrderStatus.Draft)
                throw new InvalidOperationException(ErrorCodes.Sales.INVALID_ORDER_STATUS);

            var item = SalesOrderItem.Create(productId, quantity, unitPrice);
            _items.Add(item);
            TotalAmount += quantity * unitPrice;
        }

        public void Ship()
        {
            if (Status != OrderStatus.Draft && Status != OrderStatus.Confirmed)
                throw new InvalidOperationException(ErrorCodes.Sales.INVALID_ORDER_STATUS);

            Status = OrderStatus.Shipped;
        }
    }
}
