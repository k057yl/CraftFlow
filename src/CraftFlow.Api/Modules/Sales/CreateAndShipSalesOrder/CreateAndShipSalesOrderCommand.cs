using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Sales.CreateAndShipSalesOrder;
public record CreateAndShipSalesOrderCommand(
    Guid CustomerId,
    Guid WarehouseId,
    List<SalesOrderItemRequest> Items
) : IRequest<Result<Guid>>;
