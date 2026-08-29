namespace CraftFlow.Wpf.Models;
public sealed record TraceabilityProductionBatchDto(
    Guid ProductionBatchId,
    string BatchStatus,
    DateTime StartedAt,
    DateTime? CompletedAt,
    List<TraceabilityAgingLotDto> AgingLots
);