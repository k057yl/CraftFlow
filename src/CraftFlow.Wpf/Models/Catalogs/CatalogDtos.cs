namespace CraftFlow.Wpf.Models.Catalogs;

class CatalogDtos
{
    public record RawMaterialDto(Guid Id, string Name, Guid UnitOfMeasureId, string UnitOfMeasureCode);
    public record ProductDto(Guid Id, string Name, Guid UnitOfMeasureId, string UnitOfMeasureCode);
    public record IngredientItemDto(Guid RawMaterialId, string RawMaterialName, string UnitOfMeasureCode, decimal Quantity)
    {
        public string DisplayInfo => string.IsNullOrWhiteSpace(UnitOfMeasureCode)
            ? $"{RawMaterialName} — {Quantity}"
            : $"{RawMaterialName} — {Quantity} {UnitOfMeasureCode}";
    }
}
