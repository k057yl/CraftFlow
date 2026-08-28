using CraftFlow.Api.Modules.Sales.CreateAndShipSalesOrder;
using CraftFlow.Api.Modules.Sales.CreateCustomer;
using CraftFlow.Api.Modules.Sales.GetCustomers;
using MediatR;

namespace CraftFlow.Api.Modules.Sales
{
    public static class SalesEndpoints
    {
        public static void MapSalesEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/sales")
                .WithTags("Sales");

            group.MapPost("/customers", async (CreateCustomerCommand command, ISender sender) =>
            {
                var result = await sender.Send(command);
                return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
            });

            group.MapGet("/customers", async (ISender sender) =>
            {
                var result = await sender.Send(new GetCustomersQuery());
                return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
            });

            group.MapPost("/orders/ship", async (CreateAndShipSalesOrderCommand command, ISender sender) =>
            {
                var result = await sender.Send(command);
                return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
            });
        }
    }
}
