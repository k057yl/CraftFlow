namespace CraftFlow.Api.Modules.Sales.CreateAndShipSalesOrder
{
    public record SalesOrderItemRequest(
        Guid StockLotId,
        decimal Quantity,
        decimal UnitPrice
    );
}
