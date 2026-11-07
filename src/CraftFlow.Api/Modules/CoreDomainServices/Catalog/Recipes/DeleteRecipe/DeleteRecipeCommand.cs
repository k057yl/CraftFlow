using CraftFlow.Api.Core.Authorization;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Catalog.Recipes.DeleteRecipe;

public record DeleteRecipeCommand(Guid Id) : IRequest<Result>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.RECIPES_MANAGE;
}