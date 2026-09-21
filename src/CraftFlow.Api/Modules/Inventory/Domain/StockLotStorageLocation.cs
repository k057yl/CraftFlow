namespace CraftFlow.Api.Modules.Inventory.Domain;

public sealed class StockLotStorageLocation
{
    public Guid StockLotId { get; private set; }
    public Guid StorageLocationId { get; private set; }
    public decimal AllocatedQuantity { get; private set; }

    private StockLotStorageLocation() { }

    public static StockLotStorageLocation Create(Guid stockLotId, Guid storageLocationId, decimal allocatedQuantity)
    {
        return new StockLotStorageLocation
        {
            StockLotId = stockLotId,
            StorageLocationId = storageLocationId,
            AllocatedQuantity = allocatedQuantity
        };
    }
}