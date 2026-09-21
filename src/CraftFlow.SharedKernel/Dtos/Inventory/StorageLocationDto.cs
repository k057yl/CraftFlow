namespace CraftFlow.SharedKernel.Dtos.Inventory;

public record StorageLocationDto(
    Guid Id,
    string Name,
    string LocationType,
    Guid? WarehouseId,
    Guid? ChamberId,
    decimal? Capacity,
    decimal CurrentVolume,
    bool IsOccupied,
    int BatchesProcessedCount,
    int WashCycleBatchInterval,
    DateTime? LastWashedAt,
    string MaintenanceStatus
)
{
    public decimal FreeCapacity => Capacity.HasValue ? Math.Max(0, Capacity.Value - CurrentVolume) : 0;
}