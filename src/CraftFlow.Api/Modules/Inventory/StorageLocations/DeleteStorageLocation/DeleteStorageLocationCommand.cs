using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Inventory.StorageLocations.DeleteStorageLocation;

public record DeleteStorageLocationCommand(Guid Id) : IRequest<Result<bool>>;