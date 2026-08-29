namespace CraftFlow.Api.Modules.Traceability.Contracts;
public sealed record TraceabilityProductionBatchDto(
    Guid ProductionBatchId,
    string BatchStatus,
    DateTime StartedAt,
    DateTime? CompletedAt,
    List<TraceabilityAgingLotDto> AgingLots,
    List<TraceabilityIngredientDto> UsedIngredients
);