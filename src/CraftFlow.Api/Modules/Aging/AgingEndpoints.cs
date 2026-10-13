using CraftFlow.Api.Modules.Aging.AgingProcess.DiscardAgingLot;
using CraftFlow.Api.Modules.Aging.AgingProcess.DiscardSpecificAgingItems;
using CraftFlow.Api.Modules.Aging.AgingProcess.GetActiveAgingLots;
using CraftFlow.Api.Modules.Aging.AgingProcess.GetAgingLotDetails;
using CraftFlow.Api.Modules.Aging.AgingProcess.GetAgingLotItems;
using CraftFlow.Api.Modules.Aging.AgingProcess.ReleaseFromAging;
using CraftFlow.Api.Modules.Aging.AgingProcess.TransferToAging;
using CraftFlow.Api.Modules.Aging.Chambers.CreateChamber;
using CraftFlow.Api.Modules.Aging.Chambers.DeleteChamber;
using CraftFlow.Api.Modules.Aging.Chambers.GetAgingChambers;
using MediatR;

namespace CraftFlow.Api.Modules.Aging;

public static class AgingEndpoints
{
    public static void MapAgingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(string.Empty)
            .WithTags("Aging")
            .RequireAuthorization();

        group.MapPost(AgingConstants.AGING_LOTS_TRANSFER, async (TransferToAgingCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost(AgingConstants.AGING_LOTS_RELEASE, async (ReleaseFromAgingCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
        });

        group.MapPost($"{AgingConstants.AGING_LOTS_ACTIVE}/discard", async (DiscardAgingLotCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
        });

        group.MapPost(AgingConstants.AGING_LOTS_DISCARD_ITEMS, async (DiscardSpecificAgingItemsCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
        });

        // --- AGING CHAMBERS ---
        group.MapGet(AgingConstants.AGING_CHAMBERS, async (ISender sender) =>
        {
            var result = await sender.Send(new GetAgingChambersQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost(AgingConstants.AGING_CHAMBERS, async (CreateChamberCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapDelete($"{AgingConstants.AGING_CHAMBERS}/{{id:guid}}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteChamberCommand(id));
            return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
        });

        // --- AGING LOTS ---
        group.MapGet(AgingConstants.AGING_LOTS_ACTIVE, async (GetActiveAgingLotsQueryHandler handler) =>
        {
            var result = await handler.HandleAsync();
            return Results.Ok(result);
        });

        group.MapGet($"{AgingConstants.AGING_LOTS_ACTIVE}/{{id:guid}}", async (Guid id, GetAgingLotDetailsQueryHandler handler) =>
        {
            var result = await handler.HandleAsync(id);
            return result is not null ? Results.Ok(result) : Results.NotFound();
        });

        group.MapGet($"{AgingConstants.AGING_LOTS_ACTIVE}/{{id:guid}}/items", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetAgingLotItemsQuery(id));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(result.Error);
        });
    }
}