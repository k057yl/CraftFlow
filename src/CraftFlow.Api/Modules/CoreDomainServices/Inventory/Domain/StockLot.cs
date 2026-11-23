using CraftFlow.Api.Modules.CoreDomainServices.Catalog.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Domain;

namespace CraftFlow.Api.Modules.CoreDomainServices.Inventory.Domain;

public sealed class StockLot : AggregateRoot, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public Guid ItemId { get; private set; }
    public Guid UnitOfMeasureId { get; private set; }
    public Guid? SupplierId { get; private set; }
    public decimal Quantity { get; private set; }
    public int UnitsCount { get; private set; }
    public decimal UnitPrice { get; private set; }
    public string? BatchNumber { get; private set; }
    public Guid? ProductionBatchId { get; private set; }
    public DateTime CreatedDate { get; private set; }
    public DateTime? ExpirationDate { get; private set; }
    public UnitOfMeasure UnitOfMeasure { get; private set; } = null!;

    private readonly List<StockLotStorageLocation> _storageLocations = [];
    public IReadOnlyCollection<StockLotStorageLocation> StorageLocations => _storageLocations.AsReadOnly();

    private readonly List<StockLotItem> _items = [];
    public IReadOnlyCollection<StockLotItem> Items => _items.AsReadOnly();

    private StockLot() { }

    public static StockLot Create(
        Guid tenantId,
        Guid warehouseId,
        Guid itemId,
        Guid unitOfMeasureId,
        decimal initialQuantity,
        int unitsCount,
        decimal unitPrice,
        string? batchNumber = null,
        Guid? productionBatchId = null,
        Guid? supplierId = null,
        DateTime? expirationDate = null)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException(ErrorCodes.Tenant.INVALID_TENANT_ID);

        if (initialQuantity < 0)
            throw new ArgumentException(ErrorCodes.Inventory.STOCK_LOT_NEGATIVE_QUANTITY);

        if (unitPrice < 0)
            throw new ArgumentException(ErrorCodes.Inventory.STOCK_LOT_NEGATIVE_PRICE);

        return new StockLot
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            WarehouseId = warehouseId,
            ItemId = itemId,
            UnitOfMeasureId = unitOfMeasureId,
            SupplierId = supplierId,
            Quantity = initialQuantity,
            UnitsCount = unitsCount,
            UnitPrice = unitPrice,
            BatchNumber = batchNumber,
            ProductionBatchId = productionBatchId,
            CreatedDate = DateTime.UtcNow,
            ExpirationDate = expirationDate
        };
    }

    public void PopulateItemsFromWeights(IReadOnlyList<decimal> itemWeights, string? baseCode = null)
    {
        _items.Clear();
        if (itemWeights == null || itemWeights.Count == 0) return;

        var prefix = !string.IsNullOrWhiteSpace(baseCode) ? baseCode : (BatchNumber ?? "SKU");
        for (int i = 0; i < itemWeights.Count; i++)
        {
            if (itemWeights[i] <= 0)
                throw new ArgumentException(ErrorCodes.General.VALUE_REQUIRED);

            _items.Add(StockLotItem.Create(Id, $"{prefix}-{i + 1:D2}", itemWeights[i]));
        }

        UnitsCount = _items.Count;
        Quantity = _items.Sum(i => i.CurrentWeight);
    }

    public void PopulateItemsEvenly(int unitsCount, decimal totalQuantity, string? baseCode = null)
    {
        _items.Clear();
        if (unitsCount <= 0 || totalQuantity <= 0) return;

        var prefix = !string.IsNullOrWhiteSpace(baseCode) ? baseCode : (BatchNumber ?? "SKU");
        decimal avgWeight = Math.Round(totalQuantity / unitsCount, 3);
        decimal distributed = 0m;

        for (int i = 1; i <= unitsCount; i++)
        {
            decimal weight = (i == unitsCount)
                ? Math.Round(totalQuantity - distributed, 3)
                : avgWeight;

            distributed += weight;
            _items.Add(StockLotItem.Create(Id, $"{prefix}-{i:D2}", weight));
        }

        UnitsCount = _items.Count;
        Quantity = totalQuantity;
    }

    public decimal ShipSpecificItems(IReadOnlyCollection<Guid> itemIds)
    {
        var activeItems = _items.Where(i => !i.IsSold).ToList();
        var targetSet = itemIds.ToHashSet();

        var selected = activeItems.Where(i => targetSet.Contains(i.Id)).ToList();
        if (selected.Count != targetSet.Count || selected.Count == 0)
            throw new InvalidOperationException(ErrorCodes.Sales.INSUFFICIENT_PRODUCT_STOCK);

        decimal shippedWeight = 0m;
        foreach (var item in selected)
        {
            item.MarkAsSold();
            shippedWeight += item.CurrentWeight;
        }

        Quantity = Math.Max(0m, Quantity - shippedWeight);
        UnitsCount = Math.Max(0, _items.Count(i => !i.IsSold));

        return shippedWeight;
    }

    public void AssignStorageLocation(Guid storageLocationId, decimal allocatedQuantity)
    {
        if (allocatedQuantity <= 0)
            throw new ArgumentException(ErrorCodes.Inventory.INVALID_ALLOCATED_QUANTITY);

        var existingAllocation = _storageLocations.FirstOrDefault(x => x.StorageLocationId == storageLocationId);
        var currentAllocatedTotal = _storageLocations.Where(x => x.StorageLocationId != storageLocationId).Sum(x => x.AllocatedQuantity);

        if (currentAllocatedTotal + allocatedQuantity > Quantity)
            throw new InvalidOperationException(ErrorCodes.Inventory.ALLOCATED_QUANTITY_EXCEEDS_TOTAL);

        if (existingAllocation != null)
        {
            existingAllocation.UpdateQuantity(allocatedQuantity);
        }
        else
        {
            _storageLocations.Add(StockLotStorageLocation.Create(Id, storageLocationId, allocatedQuantity));
        }
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