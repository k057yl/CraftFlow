using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Procurement.PurchaseOrders;
using CraftFlow.Api.Modules.Procurement.Suppliers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Procurement;

public static class ProcurementEndpoints
{
    public static void MapProcurementEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/procurement")
            .WithTags("Procurement");

        // --- SUPPLIERS ---
        group.MapPost("/suppliers", async (CreateSupplierCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet("/suppliers", async (AppDbContext dbContext, CancellationToken cancellationToken) =>
        {
            var suppliers = await dbContext.Suppliers
                .AsNoTracking()
                .Select(s => new { s.Id, Name = s.Name })
                .ToListAsync(cancellationToken);

            return Results.Ok(suppliers);
        });

        // --- RECEIVE GOODS / STOCK LOTS ---
        group.MapPost("/purchase-orders/receive", async (ReceiveGoodsCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
        });
    }
}