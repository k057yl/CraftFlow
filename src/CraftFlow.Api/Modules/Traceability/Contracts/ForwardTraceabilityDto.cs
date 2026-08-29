namespace CraftFlow.Api.Modules.Traceability.Contracts;

public sealed record ForwardTraceabilityDto(
    Guid RawMaterialStockLotId,
    string RawMaterialBatchNumber,
    string RawMaterialName,
    List<TraceabilityProductionBatchDto> Batches
);