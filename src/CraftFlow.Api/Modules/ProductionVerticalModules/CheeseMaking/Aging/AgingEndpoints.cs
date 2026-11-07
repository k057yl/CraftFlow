using CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Aging.AgingProcess.DiscardAgingLot;
using CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Aging.AgingProcess.DiscardSpecificAgingItems;
using CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Aging.AgingProcess.GetActiveAgingLots;
using CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Aging.AgingProcess.GetAgingLotDetails;
using CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Aging.AgingProcess.GetAgingLotItems;
using CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Aging.AgingProcess.ReleaseFromAging;
using CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Aging.AgingProcess.TransferToAging;
using CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Aging.Chambers.CreateChamber;
using CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Aging.Chambers.DeleteChamber;
using CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Aging.Chambers.GetAgingChambers;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Constants.Endpoints;
using MediatR;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Aging;

public static class AgingEndpoints
{
    public static void MapAgingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(string.Empty)
            .WithTags("Aging")
            .RequireAuthorization();

        // --- COMMANDS ---
        group.MapPost(AgingConstants.AGING_LOTS_TRANSFER, async (TransferToAgingCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : (result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : Results.BadRequest(result.Error));
        });

        group.MapPost(AgingConstants.AGING_LOTS_RELEASE, async (ReleaseFromAgingCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsSuccess
                ? Results.Ok()
                : (result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : Results.BadRequest(result.Error));
        });

        group.MapPost($"{AgingConstants.AGING_LOTS_ACTIVE}/discard", async (DiscardAgingLotCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsSuccess
                ? Results.Ok()
                : (result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : Results.BadRequest(result.Error));
        });

        group.MapPost(AgingConstants.AGING_LOTS_DISCARD_ITEMS, async (DiscardSpecificAgingItemsCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsSuccess
                ? Results.Ok()
                : (result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : Results.BadRequest(result.Error));
        });

        // --- AGING CHAMBERS ---
        group.MapGet(AgingConstants.AGING_CHAMBERS, async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetAgingChambersQuery(), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost(AgingConstants.AGING_CHAMBERS, async (CreateChamberCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : (result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : Results.BadRequest(result.Error));
        });

        group.MapDelete($"{AgingConstants.AGING_CHAMBERS}/{{id:guid}}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new DeleteChamberCommand(id), ct);
            return result.IsSuccess
                ? Results.Ok()
                : (result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : Results.BadRequest(result.Error));
        });

        // --- AGING LOTS (QUERIES VIA MEDIATR) ---
        group.MapGet(AgingConstants.AGING_LOTS_ACTIVE, async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetActiveAgingLotsQuery(), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet($"{AgingConstants.AGING_LOTS_ACTIVE}/{{id:guid}}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetAgingLotDetailsQuery(id), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(result.Error);
        });

        group.MapGet($"{AgingConstants.AGING_LOTS_ACTIVE}/{{id:guid}}/items", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetAgingLotItemsQuery(id), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(result.Error);
        });
    }
}