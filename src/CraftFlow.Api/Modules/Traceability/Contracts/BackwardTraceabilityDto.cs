namespace CraftFlow.Api.Modules.Traceability.Contracts;
public sealed record BackwardTraceabilityDto(
    Guid SalesOrderId,
    string CustomerName,
    Guid ProductStockLotId,
    string ProductBatchNumber,
    string ProductName,
    TraceabilityProductionBatchDto OriginBatch
);