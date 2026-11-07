using CraftFlow.Api.Core.Authorization;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Enums.Inventory;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Inventory.StorageLocations.CreateStorageLocation;

public record CreateStorageLocationCommand(
    string Name,
    StorageLocationType LocationType,
    Guid? WarehouseId,
    Guid? ChamberId,
    decimal? Capacity,
    string? UnitOfMeasure
) : IRequest<Result<Guid>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.WAREHOUSES_MANAGE;
}