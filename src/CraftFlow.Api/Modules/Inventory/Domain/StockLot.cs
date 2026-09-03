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
        public int UnitsCount { get; private set; }
        public decimal UnitPrice { get; private set; }
        public string? BatchNumber { get; private set; }
        public Guid? ProductionBatchId { get; private set; }
        public DateTime CreatedDate { get; private set; }

        private StockLot() { }

        public static StockLot Create(
            Guid warehouseId,
            Guid itemId,
            decimal initialQuantity,
            int unitsCount,
            decimal unitPrice,
            string? batchNumber = null,
            Guid tenantId = default,
            Guid? productionBatchId = null)
        {
            if (initialQuantity < 0)
                throw new ArgumentException(ErrorCodes.Inventory.STOCK_LOT_NEGATIVE_QUANTITY);

            if (unitPrice < 0)
                throw new ArgumentException(ErrorCodes.Inventory.UNIT_LOT_NEGATIVE_QUANTITY);

            return new StockLot
            {
                Id = Guid.NewGuid(),
                WarehouseId = warehouseId,
                ItemId = itemId,
                Quantity = initialQuantity,
                UnitsCount = unitsCount,
                UnitPrice = unitPrice,
                BatchNumber = batchNumber,
                TenantId = tenantId,
                ProductionBatchId = productionBatchId,
                CreatedDate = DateTime.UtcNow
            };
        }

        public void AdjustQuantity(decimal delta)
        {
            if (Quantity + delta < 0)
                throw new InvalidOperationException(ErrorCodes.Inventory.STOCK_LOT_NEGATIVE_QUANTITY);

            Quantity += delta;
        }

        public void SetProductionOrigin(Guid productionBatchId)
        {
            ProductionBatchId = productionBatchId;
        }
    }
}