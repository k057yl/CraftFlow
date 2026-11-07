namespace CraftFlow.Api.Modules.CoreDomainServices.Sales.Orders.CreateAndShipSalesOrder;
public record OrderItemRequest(
    Guid ProductId,
    decimal Quantity
);