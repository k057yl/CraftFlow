using CraftFlow.Api.Core.Authorization;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Inventory.StorageLocations.WriteOffStorageLocation;

public sealed record WriteOffStorageLocationCommand(
    Guid StorageLocationId,
    decimal QuantityToWriteOff,
    string? Reason
) : IRequest<Result<bool>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.INVENTORY_WRITEOFF;
}