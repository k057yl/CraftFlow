namespace CraftFlow.Api.Modules.Sales.CreateAndShipSalesOrder
{
    public record SalesOrderItemRequest(Guid ProductId, decimal Quantity, decimal UnitPrice);
}
