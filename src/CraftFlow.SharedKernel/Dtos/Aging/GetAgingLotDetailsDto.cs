namespace CraftFlow.SharedKernel.Dtos.Aging;

public record GetAgingLotDetailsDto(
    Guid LotId,
    Guid ProductId,
    string ProductName,
    string UnitName,
    decimal TotalBatchCost,
    decimal InitialQuantity,
    int UnitsCount
);