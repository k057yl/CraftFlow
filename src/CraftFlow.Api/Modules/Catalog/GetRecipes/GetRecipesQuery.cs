using CraftFlow.SharedKernel.Dtos.Catalog.Nomenclature;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Catalog.GetRecipes
{
    public record GetRecipesQuery : IRequest<Result<List<RecipeDto>>>;
}
