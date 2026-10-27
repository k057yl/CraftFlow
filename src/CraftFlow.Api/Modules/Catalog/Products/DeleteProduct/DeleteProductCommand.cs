using CraftFlow.Api.Common.Authorization;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Catalog.Products.DeleteProduct;

public record DeleteProductCommand(Guid Id) : IRequest<Result>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.RECIPES_MANAGE;
}