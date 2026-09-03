namespace CraftFlow.Api.Modules.Aging.GetAgingLotDetails;

public record GetAgingLotDetailsDto(
    Guid LotId,
    Guid ProductId,
    string ProductName,
    string UnitName,
    decimal TotalBatchCost,
    decimal InitialQuantity,
    int UnitsCount
);