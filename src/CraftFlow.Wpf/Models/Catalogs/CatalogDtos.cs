namespace CraftFlow.Wpf.Models.Catalogs;

class CatalogDtos
{
    public record IngredientItemDto(Guid RawMaterialId, string Name, string Code, decimal Quantity)
    {
        public string DisplayInfo => $"{Name} — {Quantity}";
    }
}
