using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Inventory.CreateWarehouse
{
    public record CreateWarehouseCommand(string Name, string? Address) : IRequest<Result<Guid>>;
}
