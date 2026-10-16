using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Domain;
using CraftFlow.SharedKernel.Enums.Inventory;

namespace CraftFlow.Api.Modules.Inventory.Domain;

public sealed class StorageLocation : AggregateRoot, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid? WarehouseId { get; private set; }
    public Guid? ChamberId { get; private set; }
    public string Name { get; private set; } = null!;
    public StorageLocationType LocationType { get; private set; }
    public StorageLocationStatus Status { get; private set; }

    public decimal? Capacity { get; private set; }
    public string? UnitOfMeasure { get; private set; }
    public decimal CurrentVolume { get; private set; }
    public bool IsOccupied => Status == StorageLocationStatus.OCCUPIED || CurrentVolume > 0;

    public int BatchesProcessedCount { get; private set; }
    public int WashCycleBatchInterval { get; private set; } = 1;
    public DateTime? LastWashedAt { get; private set; }
    public DateTime? NextWashDueDate { get; private set; }

    private StorageLocation() { }

    public static StorageLocation Create(
        Guid tenantId,
        string name,
        StorageLocationType locationType,
        Guid? warehouseId = null,
        Guid? chamberId = null,
        decimal? capacity = null,
        string? unitOfMeasure = null,
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
            Status = StorageLocationStatus.AVAILABLE,
            WarehouseId = warehouseId,
            ChamberId = chamberId,
            Capacity = capacity,
            UnitOfMeasure = unitOfMeasure,
            CurrentVolume = 0,
            WashCycleBatchInterval = washCycleBatchInterval,
            LastWashedAt = DateTime.UtcNow
        };
    }

    public void AddVolume(decimal volume)
    {
        if (volume > 0 && Status != StorageLocationStatus.AVAILABLE && Status != StorageLocationStatus.OCCUPIED)
        {
            throw new InvalidOperationException(ErrorCodes.Inventory.LOCATION_NOT_AVAILABLE);
        }

        if (Capacity.HasValue && (CurrentVolume + volume) > Capacity.Value)
        {
            throw new InvalidOperationException(ErrorCodes.Inventory.CAPACITY_EXCEEDED);
        }

        CurrentVolume += volume;

        if (CurrentVolume > 0)
        {
            Status = StorageLocationStatus.OCCUPIED;
        }
        else if (CurrentVolume == 0 && Status == StorageLocationStatus.OCCUPIED)
        {
            Status = StorageLocationStatus.SANITATION_REQUIRED;
        }
    }

    public void ClearVolume()
    {
        CurrentVolume = 0;
        Status = StorageLocationStatus.SANITATION_REQUIRED;
    }

    public void RegisterBatchProcessed()
    {
        BatchesProcessedCount++;

        if (BatchesProcessedCount >= WashCycleBatchInterval)
        {
            Status = StorageLocationStatus.SANITATION_REQUIRED;
        }
    }

    public void StartSanitation()
    {
        if (CurrentVolume > 0 || Status == StorageLocationStatus.OCCUPIED)
            throw new InvalidOperationException(ErrorCodes.Inventory.LOCATION_NOT_EMPTY_FOR_SANITATION);

        Status = StorageLocationStatus.UNDER_SANITATION;
    }

    public void CompleteSanitation(bool isPassed)
    {
        if (isPassed)
        {
            Status = StorageLocationStatus.AVAILABLE;
            BatchesProcessedCount = 0;
            LastWashedAt = DateTime.UtcNow;
        }
        else
        {
            Status = StorageLocationStatus.SANITATION_REQUIRED;
        }
    }

    public void SetMaintenance()
    {
        if (CurrentVolume > 0)
            throw new InvalidOperationException(ErrorCodes.Inventory.LOCATION_NOT_EMPTY_FOR_SANITATION);

        Status = StorageLocationStatus.MAINTENANCE;
    }
}