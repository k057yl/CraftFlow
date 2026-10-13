using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Inventory.CreateStorageLocation;

public record CreateStorageLocationCommand(
    string Name,
    string LocationType,
    Guid? WarehouseId = null,
    Guid? ChamberId = null,
    decimal? Capacity = null,
    string? UnitOfMeasure = null
) : IRequest<Result<Guid>>;