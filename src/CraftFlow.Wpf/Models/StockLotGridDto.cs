namespace CraftFlow.Wpf.Models;
public record StockLotGridDto(
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