using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Domain;

namespace CraftFlow.Api.Modules.Inventory.Domain;

public sealed class StorageLocation : AggregateRoot, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid? WarehouseId { get; private set; }
    public Guid? ChamberId { get; private set; }
    public string Name { get; private set; } = null!;
    public string LocationType { get; private set; } = null!;
    public decimal? Capacity { get; private set; }
    public decimal CurrentVolume { get; private set; }
    public bool IsOccupied => CurrentVolume > 0;

    public int BatchesProcessedCount { get; private set; }
    public int WashCycleBatchInterval { get; private set; } = 1;
    public DateTime? LastWashedAt { get; private set; }
    public DateTime? NextWashDueDate { get; private set; }

    private StorageLocation() { }

    public static StorageLocation Create(
        Guid tenantId,
        string name,
        string locationType,
        Guid? warehouseId = null,
        Guid? chamberId = null,
        decimal? capacity = null,
        int washCycleBatchInterval = 1)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException(ErrorCodes.Auth.INVALID_CREDENTIALS);

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(ErrorCodes.Inventory.LOCATION_NAME_REQUIRED);

        if (warehouseId == null && chamberId == null)
            throw new ArgumentException(ErrorCodes.Inventory.PARENT_CONTAINER_REQUIRED);

        return new StorageLocation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            LocationType = locationType,
            WarehouseId = warehouseId,
            ChamberId = chamberId,
            Capacity = capacity,
            CurrentVolume = 0,
            WashCycleBatchInterval = washCycleBatchInterval,
            LastWashedAt = DateTime.UtcNow
        };
    }

    public void AddVolume(decimal volume)
    {
        if (Capacity.HasValue && (CurrentVolume + volume) > Capacity.Value)
            throw new InvalidOperationException("CAPACITY_EXCEEDED");

        CurrentVolume += volume;
    }

    public void RegisterBatchProcessed()
    {
        BatchesProcessedCount++;
    }

    public void PerformSanitation()
    {
        BatchesProcessedCount = 0;
        LastWashedAt = DateTime.UtcNow;
    }
}