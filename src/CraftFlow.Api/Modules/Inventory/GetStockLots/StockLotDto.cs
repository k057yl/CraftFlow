namespace CraftFlow.Api.Modules.Inventory.GetStockLots;
public record StockLotDto(
    Guid Id,
    string ItemName,
    string WarehouseName,
    string? SupplierName,
    decimal Quantity,
    decimal UnitPrice,
    string? BatchNumber,
    DateTime CreatedDate,
    DateTime? ExpirationDate,
    bool IsExpired,
    bool IsExpiringSoon
);