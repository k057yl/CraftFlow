namespace CraftFlow.Api.Modules.Sales.GetSalesStockInfo;

public record SalesStockInfoDto(
    decimal Quantity,
    decimal UnitCost,
    decimal BasePrice
);