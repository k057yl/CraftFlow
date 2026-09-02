namespace CraftFlow.Api.Modules.Traceability.Contracts;

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