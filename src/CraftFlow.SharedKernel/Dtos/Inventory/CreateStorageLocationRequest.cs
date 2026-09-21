namespace CraftFlow.SharedKernel.Dtos.Inventory;
public record CreateStorageLocationRequest(
        string Name,
        string LocationType,
        Guid? WarehouseId = null,
        Guid? ChamberId = null,
        decimal? Capacity = null
    );