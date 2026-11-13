using CraftFlow.SharedKernel.Enums.Fermrntation;

namespace CraftFlow.SharedKernel.Dtos.Fermentation;

public record FermentationBatchDto(
    Guid Id,
    Guid ProductionBatchId,
    string BatchNumber,
    string RecipeName,
    decimal SubstrateWeightKg,
    decimal SaltPercentage,
    decimal CalculatedSaltGrams,
    decimal? InitialPh,
    decimal? CurrentPh,
    decimal TargetTemperature,
    string VesselInfo,
    FermentationStage Stage,
    DateTime StartedAt,
    DateTime? MovedToColdAt,
    DateTime? CompletedAt,
    int DaysElapsed,
    List<FermentationReadingDto> Readings
);