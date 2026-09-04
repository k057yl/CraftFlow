namespace CraftFlow.Api.Modules.Production.GetBatchCost;

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