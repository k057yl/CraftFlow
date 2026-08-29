using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Catalog.CreateRecipe;
public record CreateRecipeCommand(
    Guid ProductId,
    string Name,
    decimal TargetOutputQuantity,
    List<RecipeIngredientDto> Ingredients,
    bool IsAgingRequired = false, 
    int? DefaultMinAgingDays = null
) : IRequest<Result<Guid>>;
