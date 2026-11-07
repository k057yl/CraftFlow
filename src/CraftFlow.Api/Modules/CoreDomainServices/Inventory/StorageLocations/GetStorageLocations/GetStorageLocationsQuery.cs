using CraftFlow.SharedKernel.Dtos.Catalog.Inventory;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Inventory.StorageLocations.GetStorageLocations;
public record GetStorageLocationsQuery(
    Guid? WarehouseId = null,
    Guid? ChamberId = null
) : IRequest<Result<List<StorageLocationDto>>>;