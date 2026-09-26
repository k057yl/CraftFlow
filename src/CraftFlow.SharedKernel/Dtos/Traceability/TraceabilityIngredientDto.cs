namespace CraftFlow.SharedKernel.Dtos.Traceability;

public sealed record TraceabilityIngredientDto(
    Guid RawMaterialStockLotId,
    string RawMaterialName,
    string BatchNumber,
    decimal QuantityUsed,
    string UnitOfMeasure,
    string SupplierName
);