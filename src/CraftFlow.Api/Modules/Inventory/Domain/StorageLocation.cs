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

    public Guid? CurrentProductionBatchId { get; private set; }

    public bool IsOccupied => CurrentVolume > 0
                           || Status == StorageLocationStatus.OCCUPIED
                           || CurrentProductionBatchId.HasValue;

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
            throw new ArgumentException(ErrorCodes.Tenant.INVALID_TENANT_ID);

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(ErrorCodes.Inventory.LOCATION_NAME_REQUIRED);

        if (warehouseId == null && chamberId == null)
            throw new ArgumentException(ErrorCodes.Inventory.PARENT_CONTAINER_REQUIRED);

        if (warehouseId != null && chamberId != null)
            throw new ArgumentException(ErrorCodes.Inventory.AMBIGUOUS_PARENT_CONTAINER);

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
            WashCycleBatchInterval = washCycleBatchInterval > 0 ? washCycleBatchInterval : 1,
            LastWashedAt = DateTime.UtcNow
        };
    }

    public void LockForProduction(Guid productionBatchId)
    {
        if (productionBatchId == Guid.Empty)
            throw new ArgumentException(ErrorCodes.Inventory.INVALID_PRODUCTION_BATCH_ID);

        if (Status != StorageLocationStatus.AVAILABLE)
            throw new InvalidOperationException(ErrorCodes.Inventory.LOCATION_NOT_AVAILABLE);

        CurrentProductionBatchId = productionBatchId;
        Status = StorageLocationStatus.OCCUPIED;
    }

    public void ReleaseFromProduction()
    {
        if (!CurrentProductionBatchId.HasValue && Status != StorageLocationStatus.OCCUPIED)
            throw new InvalidOperationException(ErrorCodes.Inventory.LOCATION_NOT_AVAILABLE);

        CurrentProductionBatchId = null;
        CurrentVolume = 0;
        Status = StorageLocationStatus.SANITATION_REQUIRED;
    }

    public void AddVolume(decimal volume)
    {
        if (volume == 0)
            return;

        CurrentVolume += volume;

        if (CurrentVolume <= 0)
        {
            CurrentVolume = 0;
            CurrentProductionBatchId = null;
            Status = StorageLocationStatus.SANITATION_REQUIRED;
        }
        else
        {
            Status = StorageLocationStatus.OCCUPIED;
        }
    }

    public void ClearVolume()
    {
        CurrentVolume = 0;
        CurrentProductionBatchId = null;

        if (Status == StorageLocationStatus.OCCUPIED)
        {
            Status = StorageLocationStatus.SANITATION_REQUIRED;
        }
    }

    public void RegisterBatchProcessed()
    {
        BatchesProcessedCount++;

        if (BatchesProcessedCount >= WashCycleBatchInterval && CurrentVolume == 0)
        {
            Status = StorageLocationStatus.SANITATION_REQUIRED;
        }
    }

    public void StartSanitation()
    {
        bool isShelfOrRack = LocationType == StorageLocationType.RACK_SHELF;

        if (!isShelfOrRack && (CurrentVolume > 0 || CurrentProductionBatchId.HasValue))
            throw new InvalidOperationException(ErrorCodes.Inventory.LOCATION_HAS_VOLUME);

        if (Status != StorageLocationStatus.SANITATION_REQUIRED && Status != StorageLocationStatus.AVAILABLE && Status != StorageLocationStatus.OCCUPIED)
            throw new InvalidOperationException(ErrorCodes.Inventory.LOCATION_NOT_EMPTY_FOR_SANITATION);

        Status = StorageLocationStatus.UNDER_SANITATION;
    }

    public void CompleteSanitation(bool isPassed)
    {
        if (isPassed)
        {
            Status = CurrentVolume > 0 ? StorageLocationStatus.OCCUPIED : StorageLocationStatus.AVAILABLE;
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
        if (CurrentVolume > 0 || CurrentProductionBatchId.HasValue)
            throw new InvalidOperationException(ErrorCodes.Inventory.LOCATION_HAS_VOLUME);

        Status = StorageLocationStatus.MAINTENANCE;
    }
}