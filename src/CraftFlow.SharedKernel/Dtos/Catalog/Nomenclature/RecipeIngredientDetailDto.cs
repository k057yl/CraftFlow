namespace CraftFlow.SharedKernel.Dtos.Catalog.Nomenclature;

public record RecipeIngredientDetailDto(
    Guid RawMaterialId,
    string RawMaterialName,
    string UnitOfMeasureCode,
    decimal Quantity
);