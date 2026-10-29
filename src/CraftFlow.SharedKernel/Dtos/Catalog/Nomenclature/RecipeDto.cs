namespace CraftFlow.SharedKernel.Dtos.Catalog.Nomenclature;

public record RecipeDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string Name,
    decimal TargetOutputQuantity,
    bool IsAgingRequired,
    int? DefaultMinAgingDays,
    List<RecipeIngredientDetailDto> Ingredients,
    bool IsArchived = false
);