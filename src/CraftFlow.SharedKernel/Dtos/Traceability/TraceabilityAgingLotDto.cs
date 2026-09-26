namespace CraftFlow.SharedKernel.Dtos.Traceability;

public sealed record TraceabilityAgingLotDto(
    Guid AgingLotId,
    string AgingBatchNumber,
    string ChamberName,
    string AgingStatus
);