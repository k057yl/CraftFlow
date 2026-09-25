namespace CraftFlow.SharedKernel.Dtos.Sale;

public sealed record SalesOrderItemRequestDto(
    Guid StockLotId,
    decimal Quantity,
    decimal UnitPrice
);