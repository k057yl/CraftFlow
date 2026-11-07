using CraftFlow.SharedKernel.Dtos.Catalog.Inventory;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Inventory.Warehouses.GetWarehouses
{
    public record GetWarehousesQuery : IRequest<Result<List<WarehouseDto>>>;
}
