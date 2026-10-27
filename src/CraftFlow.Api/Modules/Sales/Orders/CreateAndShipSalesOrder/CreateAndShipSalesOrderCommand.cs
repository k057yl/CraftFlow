using CraftFlow.Api.Common.Authorization;
using CraftFlow.SharedKernel.Dtos.Sale;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Sales.Orders.CreateAndShipSalesOrder;

public sealed record CreateAndShipSalesOrderCommand(
    Guid CustomerId,
    Guid WarehouseId,
    List<SalesOrderItemRequestDto> Items
) : IRequest<Result<Guid>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.SALES_MANAGE;
}