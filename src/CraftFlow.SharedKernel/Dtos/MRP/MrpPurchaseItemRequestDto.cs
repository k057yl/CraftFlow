namespace CraftFlow.SharedKernel.Dtos.MRP;

public sealed record MrpPurchaseItemRequestDto(
    Guid RawMaterialId,
    decimal Quantity
);