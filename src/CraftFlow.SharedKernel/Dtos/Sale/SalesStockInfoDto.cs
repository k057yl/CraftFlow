namespace CraftFlow.SharedKernel.Dtos.Sale;

public record SalesStockInfoDto(
    decimal Quantity,
    decimal UnitCost,
    decimal BasePrice
);