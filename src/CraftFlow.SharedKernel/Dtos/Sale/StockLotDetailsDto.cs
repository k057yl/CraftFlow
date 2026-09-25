using CraftFlow.SharedKernel.Dtos.Sale;

namespace CraftFlow.Api.Modules.Sales.GetStockLotDetails;

public sealed record StockLotDetailsDto(
    Guid StockLotId,
    string BatchNumber,
    string ProductName,
    decimal TotalQuantity,
    int UnitsCount,
    List<LotItemDetailsDto> Items
);