using CraftFlow.SharedKernel.Dtos.Inventory;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Inventory.GetStorageLocations;
public record GetStorageLocationsQuery(
    Guid? WarehouseId = null,
    Guid? ChamberId = null
) : IRequest<Result<List<StorageLocationDto>>>;