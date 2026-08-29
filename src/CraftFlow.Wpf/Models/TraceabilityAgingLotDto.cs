namespace CraftFlow.Wpf.Models;
public sealed record TraceabilityAgingLotDto(
    Guid AgingLotId,
    string AgingBatchNumber,
    string ChamberName,
    string AgingStatus
);
