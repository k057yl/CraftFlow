using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Inventory.GetWarehouses
{
    public record GetWarehousesQuery : IRequest<Result<List<WarehouseDto>>>;
}
