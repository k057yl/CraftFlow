namespace CraftFlow.Api.Modules.Sales.Orders.CreateAndShipSalesOrder;
public record OrderItemRequest(
    Guid ProductId,
    decimal Quantity
);