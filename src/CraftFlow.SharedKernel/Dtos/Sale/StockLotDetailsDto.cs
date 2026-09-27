namespace CraftFlow.SharedKernel.Dtos.Sale;

public sealed record StockLotDetailsDto(
    Guid StockLotId,
    string BatchNumber,
    string ProductName,
    decimal TotalQuantity,
    int UnitsCount,
    List<LotItemDetailsDto> Items
);