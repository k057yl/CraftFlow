using CraftFlow.Api.Core.Extensions;
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
            (await sender.Send(command, ct)).ToHttpResult());

        group.MapPost(AgingConstants.AGING_LOTS_RELEASE, async (ReleaseFromAgingCommand command, ISender sender, CancellationToken ct) =>
            (await sender.Send(command, ct)).ToHttpResult());

        group.MapPost($"{AgingConstants.AGING_LOTS_ACTIVE}/discard", async (DiscardAgingLotCommand command, ISender sender, CancellationToken ct) =>
            (await sender.Send(command, ct)).ToHttpResult());

        group.MapPost(AgingConstants.AGING_LOTS_DISCARD_ITEMS, async (DiscardSpecificAgingItemsCommand command, ISender sender, CancellationToken ct) =>
            (await sender.Send(command, ct)).ToHttpResult());

        // --- AGING CHAMBERS ---
        group.MapGet(AgingConstants.AGING_CHAMBERS, async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetAgingChambersQuery(), ct)).ToHttpResult());

        group.MapPost(AgingConstants.AGING_CHAMBERS, async (CreateChamberCommand command, ISender sender, CancellationToken ct) =>
            (await sender.Send(command, ct)).ToHttpResult());

        group.MapDelete($"{AgingConstants.AGING_CHAMBERS}/{{id:guid}}", async (Guid id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new DeleteChamberCommand(id), ct)).ToHttpResult());

        // --- AGING LOTS (QUERIES VIA MEDIATR) ---
        group.MapGet(AgingConstants.AGING_LOTS_ACTIVE, async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetActiveAgingLotsQuery(), ct)).ToHttpResult());

        group.MapGet($"{AgingConstants.AGING_LOTS_ACTIVE}/{{id:guid}}", async (Guid id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetAgingLotDetailsQuery(id), ct)).ToHttpResult(notFoundOnError: true));

        group.MapGet($"{AgingConstants.AGING_LOTS_ACTIVE}/{{id:guid}}/items", async (Guid id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetAgingLotItemsQuery(id), ct)).ToHttpResult(notFoundOnError: true));
    }
}