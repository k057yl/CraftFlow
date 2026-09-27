namespace CraftFlow.SharedKernel.Dtos.Aging;

public record ReleaseFromAgingRequest(
    Guid AgingLotId,
    Guid TargetWarehouseId,
    decimal ActualFinalQuantity,
    int UnitsCount,
    decimal UnitPrice,
    string? CustomBatchNumber,
    List<Guid>? StorageLocationIds
);