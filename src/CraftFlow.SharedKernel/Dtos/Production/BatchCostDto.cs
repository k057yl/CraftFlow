namespace CraftFlow.SharedKernel.Dtos.Production;

public record BatchCostDto(
    Guid BatchId,
    string RecipeName,
    decimal PlannedOutputQuantity,
    decimal ActualOutputQuantity,
    decimal TotalRawMaterialCost,
    decimal TotalCostWithOverhead,
    decimal? OverheadPercentage,
    decimal UnitCost
);