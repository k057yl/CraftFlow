namespace CraftFlow.SharedKernel.Dtos.MRP;

public sealed record BatchMaterialRequirementDto(
    Guid RawMaterialId,
    string RawMaterialName,
    decimal TotalRequiredQuantity,
    decimal CurrentStockQuantity,
    decimal ShortageQuantity
);