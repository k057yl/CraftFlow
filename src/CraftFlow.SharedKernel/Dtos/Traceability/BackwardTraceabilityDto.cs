namespace CraftFlow.SharedKernel.Dtos.Traceability;

public sealed record BackwardTraceabilityDto(
    Guid? SalesOrderId,
    string CustomerName,
    Guid ProductStockLotId,
    string ProductBatchNumber,
    string ProductName,
    decimal CurrentStockQuantity,
    decimal UnitPrice,
    int AgingDaysTotal,
    decimal AgingLossPercentage,
    string StorageChamberName,
    TraceabilityProductionBatchDto OriginBatch
);