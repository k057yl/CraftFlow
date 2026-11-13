using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Domain;
using CraftFlow.SharedKernel.Enums.Fermrntation;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.Fermentation.Domain;

public sealed class FermentationBatch : AggregateRoot, ITenantEntity
{
    private readonly List<FermentationReading> _readings = [];

    public Guid TenantId { get; private set; }
    public Guid ProductionBatchId { get; private set; }
    public string BatchNumber { get; private set; } = null!;
    public string RecipeName { get; private set; } = null!;

    public decimal SubstrateWeightKg { get; private set; }
    public decimal SaltPercentage { get; private set; }
    public decimal CalculatedSaltGrams => Math.Round(SubstrateWeightKg * 1000m * (SaltPercentage / 100m), 1);

    public decimal? InitialPh { get; private set; }
    public decimal? CurrentPh { get; private set; }
    public decimal TargetTemperature { get; private set; }
    public string VesselInfo { get; private set; } = string.Empty;

    public FermentationStage Stage { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? MovedToColdAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public IReadOnlyCollection<FermentationReading> Readings => _readings.AsReadOnly();

    private FermentationBatch() { }

    public static FermentationBatch Create(
        Guid tenantId,
        Guid productionBatchId,
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
            ProductionBatchId = productionBatchId,
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

    public void Complete()
    {
        if (Stage == FermentationStage.DISCARDED)
            throw new InvalidOperationException(ErrorCodes.Fermentation.CANNOT_COMPLETE_DISCARDED);

        Stage = FermentationStage.COMPLETED;
        CompletedAt = DateTime.UtcNow;
    }
}