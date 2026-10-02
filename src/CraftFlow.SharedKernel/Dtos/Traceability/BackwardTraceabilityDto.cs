namespace CraftFlow.SharedKernel.Dtos.Traceability;

public sealed record BackwardTraceabilityDto(
    Guid? SalesOrderId,
    Guid? CustomerId,
    string CustomerName,
    Guid ProductStockLotId,
    string ProductBatchNumber,
    string ProductName,
    decimal CurrentStockQuantity,
    int AgingDaysTotal,
    decimal AgingLossPercentage,
    string StorageChamberName,
    TraceabilityProductionBatchDto OriginBatch
);