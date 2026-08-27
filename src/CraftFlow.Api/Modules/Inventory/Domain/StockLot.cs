using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Domain;

namespace CraftFlow.Api.Modules.Inventory.Domain
{
    public sealed class StockLot : AggregateRoot, ITenantEntity
    {
        public Guid TenantId { get; private set; }
        public Guid WarehouseId { get; private set; }
        public Guid ItemId { get; private set; }
        public decimal Quantity { get; private set; }
        public string? BatchNumber { get; private set; }

        private StockLot() { }

        public static StockLot Create(Guid warehouseId, Guid itemId, decimal initialQuantity, string? batchNumber = null)
        {
            if (initialQuantity < 0)
                throw new ArgumentException(ErrorCodes.Inventory.STOCK_LOT_NEGATIVE_QUANTITY);

            return new StockLot
            {
                Id = Guid.NewGuid(),
                WarehouseId = warehouseId,
                ItemId = itemId,
                Quantity = initialQuantity,
                BatchNumber = batchNumber
            };
        }

        public void AdjustQuantity(decimal delta)
        {
            if (Quantity + delta < 0)
                throw new InvalidOperationException(ErrorCodes.Inventory.STOCK_LOT_NEGATIVE_QUANTITY);

            Quantity += delta;
        }
    }
}
