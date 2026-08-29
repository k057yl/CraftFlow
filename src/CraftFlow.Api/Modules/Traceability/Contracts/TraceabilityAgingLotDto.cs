namespace CraftFlow.Api.Modules.Traceability.Contracts;
public sealed record TraceabilityAgingLotDto(
    Guid AgingLotId,
    string AgingBatchNumber,
    string ChamberName,
    string AgingStatus
);