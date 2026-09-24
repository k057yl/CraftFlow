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
    public decimal FreeCapacity => Capacity.HasValue ? Math.Max(0m, Capacity.Value - CurrentVolume) : 0m;

    public string VolumeInfo => Capacity.HasValue && Capacity.Value > 0
        ? $"{CurrentVolume:N0} / {Capacity.Value:N0} л (Свободно: {FreeCapacity:N0} л)"
        : $"{CurrentVolume:N0} л";
}