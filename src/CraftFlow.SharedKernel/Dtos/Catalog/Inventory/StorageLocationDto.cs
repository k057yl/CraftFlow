namespace CraftFlow.SharedKernel.Dtos.Inventory;

public record StorageLocationDto(
    Guid Id,
    string Name,
    string LocationType,
    Guid? WarehouseId,
    string? WarehouseName,
    Guid? ChamberId,
    string? ChamberName,
    decimal? Capacity,
    decimal CurrentVolume,
    bool IsOccupied,
    int BatchesProcessedCount,
    int WashCycleBatchInterval,
    DateTime? LastWashedAt,
    string MaintenanceStatus,
    string? UnitOfMeasure = null
)
{
    public decimal FreeCapacity => Capacity.HasValue
        ? Math.Max(0m, Capacity.Value - CurrentVolume)
        : decimal.MaxValue;

    public string FreeCapacityDisplay => Capacity.HasValue
        ? $"{FreeCapacity:N0}"
        : "∞";

    public string VolumeInfo
    {
        get
        {
            var uom = !string.IsNullOrWhiteSpace(UnitOfMeasure) ? UnitOfMeasure : "л";
            return Capacity.HasValue && Capacity.Value > 0
                ? $"{CurrentVolume:N0} / {Capacity.Value:N0} {uom} (Свободно: {FreeCapacity:N0} {uom})"
                : $"{CurrentVolume:N0} {uom} (Без лимита)";
        }
    }

    public string ParentName => !string.IsNullOrWhiteSpace(WarehouseName)
        ? $"🏠 {WarehouseName}"
        : (!string.IsNullOrWhiteSpace(ChamberName) ? $"❄ {ChamberName}" : "—");
}