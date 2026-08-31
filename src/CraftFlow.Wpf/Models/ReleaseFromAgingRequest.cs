namespace CraftFlow.Wpf.Models;

public record ReleaseFromAgingRequest(
    Guid AgingLotId,
    Guid DestinationWarehouseId,
    decimal ActualQuantity,
    decimal UnitPrice
);