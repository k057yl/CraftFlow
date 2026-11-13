using CraftFlow.SharedKernel.Enums;

namespace CraftFlow.SharedKernel.Dtos.Catalog.Nomenclature;

public record RecipeDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string Name,
    AppModule TargetModule,
    decimal TargetOutputQuantity,
    bool IsAgingRequired,
    int? DefaultMinAgingDays,
    List<RecipeIngredientDetailDto> Ingredients,
    bool IsArchived = false
);