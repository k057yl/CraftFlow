namespace CraftFlow.SharedKernel.Dtos.Catalog.Inventory;
public record CreateStorageLocationRequest(
        string Name,
        string LocationType,
        Guid? WarehouseId = null,
        Guid? ChamberId = null,
        decimal? Capacity = null
    );