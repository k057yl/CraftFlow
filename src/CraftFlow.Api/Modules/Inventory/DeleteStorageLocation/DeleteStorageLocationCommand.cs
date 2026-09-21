using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Inventory.DeleteStorageLocation;

public record DeleteStorageLocationCommand(Guid Id) : IRequest<Result<bool>>;