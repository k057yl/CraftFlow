namespace CraftFlow.SharedKernel.Dtos.Catalog.Nomenclature;

public record RecipeDto(Guid Id, string Name, bool IsAgingRequired, int? DefaultMinAgingDays);