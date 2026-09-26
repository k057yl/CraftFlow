namespace CraftFlow.SharedKernel.Dtos.Traceability;

public sealed record TraceabilityProductionBatchDto(
    Guid ProductionBatchId,
    int BatchStatusInt,
    DateTime StartedAt,
    DateTime? CompletedAt,
    decimal PlannedQuantity,
    decimal ActualOutputQuantity,
    decimal OutputYieldPercentage,
    List<TraceabilityIngredientDto> ConsumedIngredients
);