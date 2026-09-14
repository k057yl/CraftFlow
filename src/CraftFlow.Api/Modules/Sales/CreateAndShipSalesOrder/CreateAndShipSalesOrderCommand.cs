using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Sales.CreateAndShipSalesOrder;
public record CreateAndShipSalesOrderCommand(
    Guid ProductId,
    Guid CustomerId,
    Guid WarehouseId,
    List<OrderItemRequest> Items
) : IRequest<Result<Guid>>;
