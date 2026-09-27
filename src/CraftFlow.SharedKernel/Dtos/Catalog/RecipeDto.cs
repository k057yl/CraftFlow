namespace CraftFlow.SharedKernel.Dtos.Catalog;

public record RecipeDto(Guid Id, string Name, bool IsAgingRequired, int? DefaultMinAgingDays);