namespace CraftFlow.SharedKernel.Dtos.MRP;

public sealed record SummaryRequirementDto(
    Guid RawMaterialId,
    string RawMaterialName,
    decimal TotalRequiredQuantity,
    decimal CurrentStockQuantity,
    decimal ShortageQuantity
);