namespace CraftFlow.Api.Modules.Sales.CreateAndShipSalesOrder;
public record OrderItemRequest(
    Guid ProductId,
    decimal Quantity
);