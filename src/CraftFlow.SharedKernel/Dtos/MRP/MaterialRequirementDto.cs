namespace CraftFlow.SharedKernel.Dtos.MRP;

public sealed record MaterialRequirementDto(
    Guid RawMaterialId,
    string RawMaterialName,
    decimal TotalRequiredQuantity,
    decimal CurrentStockQuantity,
    decimal ShortageQuantity,
    decimal SuggestedPurchaseQuantity
);