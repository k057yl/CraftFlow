namespace CraftFlow.Wpf.Models;
public sealed record ReleaseFromAgingRequest(Guid AgingLotId, Guid TargetWarehouseId, decimal ActualFinalQuantity, decimal UnitPrice);