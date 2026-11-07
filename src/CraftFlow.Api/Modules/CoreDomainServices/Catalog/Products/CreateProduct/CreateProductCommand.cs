using CraftFlow.Api.Core.Authorization;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Catalog.Products.CreateProduct;

public record CreateProductCommand(string Name, Guid UnitOfMeasureId) : IRequest<Result<Guid>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.RECIPES_MANAGE;
}