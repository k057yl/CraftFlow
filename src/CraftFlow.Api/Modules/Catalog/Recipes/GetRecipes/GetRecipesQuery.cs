using CraftFlow.SharedKernel.Dtos.Catalog.Nomenclature;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Catalog.Recipes.GetRecipes
{
    public record GetRecipesQuery(bool IncludeArchived = false) : IRequest<Result<List<RecipeDto>>>;
}
