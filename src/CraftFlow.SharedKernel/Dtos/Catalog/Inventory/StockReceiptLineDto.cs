namespace CraftFlow.SharedKernel.Dtos.Catalog.Inventory;

public sealed record StockReceiptLineDto(
    Guid ItemId,
    decimal Quantity,
    decimal UnitPrice,
    Guid UnitOfMeasureId = default,
    string? BatchNumber = null,
    DateTime? ExpirationDate = null,
    List<Guid>? StorageLocationIds = null
);