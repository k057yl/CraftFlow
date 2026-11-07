using CraftFlow.Api.Core.Authorization;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Inventory.StorageLocations.DeleteStorageLocation;

public record DeleteStorageLocationCommand(Guid Id) : IRequest<Result<bool>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.WAREHOUSES_MANAGE;
}