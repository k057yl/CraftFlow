using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Inventory.DeleteWarehouse;

public record DeleteWarehouseCommand(Guid Id) : IRequest<Result<bool>>;