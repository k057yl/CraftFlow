using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Domain;
using CraftFlow.SharedKernel.Enums.Fermrntation;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.Fermentation.Domain;

public sealed class FermentationBatch : AggregateRoot, ITenantEntity
{
    private readonly List<FermentationReading> _readings = [];
    private readonly List<FermentationConsumedIngredient> _consumedIngredients = [];

    public Guid TenantId { get; private set; }
    public Guid RecipeId { get; private set; }
    public Guid TargetProductId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public Guid DestinationWarehouseId { get; private set; }
    public Guid EquipmentLocationId { get; private set; }

    public string BatchNumber { get; private set; } = null!;
    public string RecipeName { get; private set; } = null!;

    public decimal SubstrateWeightKg { get; private set; }
    public decimal SaltPercentage { get; private set; }
    public decimal CalculatedSaltGrams => Math.Round(SubstrateWeightKg * 1000m * (SaltPercentage / 100m), 1);

    public decimal? InitialPh { get; private set; }
    public decimal? CurrentPh { get; private set; }
    public decimal TargetTemperature { get; private set; }
    public string VesselInfo { get; private set; } = string.Empty;

    public decimal ActualOutputQuantity { get; private set; }
    public int UnitsCount { get; private set; }
    public decimal? OverheadPercentage { get; private set; }
    public string? DiscardReason { get; private set; }

    public FermentationStage Stage { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? MovedToColdAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? DiscardedAt { get; private set; }

    public IReadOnlyCollection<FermentationReading> Readings => _readings.AsReadOnly();
    public IReadOnlyCollection<FermentationConsumedIngredient> ConsumedIngredients => _consumedIngredients.AsReadOnly();

    private FermentationBatch() { }

    public static FermentationBatch Create(
        Guid tenantId,
        Guid recipeId,
        Guid targetProductId,
        Guid warehouseId,
        Guid destinationWarehouseId,
        Guid equipmentLocationId,
        string batchNumber,
        string recipeName,
        decimal substrateWeightKg,
        decimal saltPercentage,
        decimal targetTemperature,
        string vesselInfo,
        decimal? initialPh = null)
    {
        if (substrateWeightKg <= 0)
            throw new ArgumentException(ErrorCodes.General.VALUE_REQUIRED);

        return new FermentationBatch
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RecipeId = recipeId,
            TargetProductId = targetProductId,
            WarehouseId = warehouseId,
            DestinationWarehouseId = destinationWarehouseId != Guid.Empty ? destinationWarehouseId : warehouseId,
            EquipmentLocationId = equipmentLocationId,
            BatchNumber = batchNumber,
            RecipeName = recipeName,
            SubstrateWeightKg = substrateWeightKg,
            SaltPercentage = saltPercentage,
            TargetTemperature = targetTemperature,
            VesselInfo = vesselInfo,
            InitialPh = initialPh,
            CurrentPh = initialPh,
            Stage = FermentationStage.ACTIVE_WARM,
            StartedAt = DateTime.UtcNow
        };
    }

    public void AddConsumedIngredient(Guid stockLotId, Guid rawMaterialId, decimal quantity)
    {
        _consumedIngredients.Add(FermentationConsumedIngredient.Create(Id, stockLotId, rawMaterialId, quantity, TenantId));
    }

    public FermentationReading AddReading(
        decimal? ph,
        decimal? temperature,
        bool degassedOrBurped,
        bool brineToppedUp,
        string? sensoryNotes)
    {
        if (Stage is FermentationStage.COMPLETED or FermentationStage.DISCARDED)
            throw new InvalidOperationException(ErrorCodes.Fermentation.BATCH_ALREADY_COMPLETED);

        if (ph.HasValue)
            CurrentPh = ph.Value;

        var reading = FermentationReading.Create(Id, ph, temperature, degassedOrBurped, brineToppedUp, sensoryNotes);
        _readings.Add(reading);
        return reading;
    }

    public void MoveToColdStorage()
    {
        if (Stage != FermentationStage.ACTIVE_WARM)
            throw new InvalidOperationException(ErrorCodes.Fermentation.INVALID_STAGE_FOR_COLD);

        Stage = FermentationStage.COLD_RETARDATION;
        MovedToColdAt = DateTime.UtcNow;
    }

    public void Complete(decimal actualOutputQuantity, int unitsCount, decimal? overheadPercentage, string? batchNumber)
    {
        if (Stage == FermentationStage.DISCARDED)
            throw new InvalidOperationException(ErrorCodes.Fermentation.CANNOT_COMPLETE_DISCARDED);

        if (Stage == FermentationStage.COMPLETED)
            throw new InvalidOperationException(ErrorCodes.Fermentation.BATCH_ALREADY_COMPLETED);

        if (!string.IsNullOrWhiteSpace(batchNumber))
            BatchNumber = batchNumber;

        ActualOutputQuantity = actualOutputQuantity;
        UnitsCount = unitsCount > 0 ? unitsCount : 1;
        OverheadPercentage = overheadPercentage;
        Stage = FermentationStage.COMPLETED;
        CompletedAt = DateTime.UtcNow;
    }

    public void Discard(string reason)
    {
        if (Stage is FermentationStage.COMPLETED or FermentationStage.DISCARDED)
            throw new InvalidOperationException(ErrorCodes.Fermentation.BATCH_ALREADY_COMPLETED);

        DiscardReason = reason;
        Stage = FermentationStage.DISCARDED;
        DiscardedAt = DateTime.UtcNow;
    }
}