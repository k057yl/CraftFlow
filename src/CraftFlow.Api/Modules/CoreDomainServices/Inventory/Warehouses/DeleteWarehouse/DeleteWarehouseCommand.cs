using CraftFlow.Api.Core.Authorization;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Inventory.Warehouses.DeleteWarehouse;

public record DeleteWarehouseCommand(Guid Id) : IRequest<Result<bool>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.WAREHOUSES_MANAGE;
}