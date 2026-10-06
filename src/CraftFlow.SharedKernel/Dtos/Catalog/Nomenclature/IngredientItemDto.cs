namespace CraftFlow.SharedKernel.Dtos.Catalog.Nomenclature;

public record IngredientItemDto(
    Guid RawMaterialId,
    string RawMaterialName,
    string UnitOfMeasureCode,
    decimal Quantity
)
{
    public string DisplayInfo => string.IsNullOrWhiteSpace(UnitOfMeasureCode)
        ? $"{RawMaterialName} — {Quantity}"
        : $"{RawMaterialName} — {Quantity} {UnitOfMeasureCode}";
}