namespace CraftFlow.SharedKernel.Dtos.Catalog.Inventory;

public sealed record ReceiveStockBatchRequest(
    Guid SupplierId,
    Guid WarehouseId,
    string? InvoiceNumber,
    List<StockReceiptLineDto> Items
);