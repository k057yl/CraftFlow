using CraftFlow.Api.Core.Authorization;
using CraftFlow.SharedKernel.Dtos.Catalog.Nomenclature;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Catalog.Recipes.CreateRecipe;

public record CreateRecipeCommand(
    Guid ProductId,
    string Name,
    decimal TargetOutputQuantity,
    List<RecipeIngredientDto> Ingredients,
    bool IsAgingRequired = false,
    int? DefaultMinAgingDays = null
) : IRequest<Result<Guid>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.RECIPES_MANAGE;
}