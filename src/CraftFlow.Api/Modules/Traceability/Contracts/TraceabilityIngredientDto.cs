namespace CraftFlow.Api.Modules.Traceability.Contracts;
public sealed record TraceabilityIngredientDto(
    Guid RawMaterialStockLotId,
    string RawMaterialName,
    string BatchNumber,
    decimal QuantityUsed
);