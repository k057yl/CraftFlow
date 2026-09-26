namespace CraftFlow.SharedKernel.Dtos.Traceability;

public sealed record ForwardTraceabilityDto(
    Guid RawMaterialStockLotId,
    string RawMaterialBatchNumber,
    string RawMaterialName,
    List<TraceabilityProductionBatchDto> Batches
);