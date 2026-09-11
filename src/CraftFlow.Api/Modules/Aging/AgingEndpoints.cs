using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Aging.CreateChamber;
using CraftFlow.Api.Modules.Aging.DeleteChamber;
using CraftFlow.Api.Modules.Aging.Domain;
using CraftFlow.Api.Modules.Aging.GetActiveAgingLots;
using CraftFlow.Api.Modules.Aging.GetAgingLotDetails;
using CraftFlow.Api.Modules.Aging.ReleaseFromAging;
using CraftFlow.Api.Modules.Aging.TransferToAging;
using CraftFlow.Api.Modules.Production.Domain;
using CraftFlow.SharedKernel.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Aging;

public static class AgingEndpoints
{
    public static void MapAgingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("")
            .WithTags("Aging")
            .RequireAuthorization();

        group.MapPost(Endpoints.AGING_LOTS_TRANSFER, async (TransferToAgingCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost(Endpoints.AGING_LOTS_RELEASE, async (ReleaseFromAgingCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
        });

        // --- AGING CHAMBERS ---
        group.MapGet(Endpoints.AGING_CHAMBERS, async (AppDbContext dbContext, CancellationToken cancellationToken) =>
        {
            var chambers = await dbContext.AgingChambers
                .AsNoTracking()
                .Select(c => new { c.Id, c.Name })
                .ToListAsync(cancellationToken);

            return Results.Ok(chambers);
        });

        group.MapPost(Endpoints.AGING_CHAMBERS, async (CreateChamberCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapDelete($"{Endpoints.AGING_CHAMBERS}/{{id:guid}}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteChamberCommand(id));
            return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
        });

        // --- AGING LOTS ---
        group.MapGet(Endpoints.AGING_LOTS_ACTIVE, async (AppDbContext dbContext, CancellationToken cancellationToken) =>
        {
            var activeLots = await dbContext.AgingLots
                .AsNoTracking()
                .Where(l => l.State == AgingState.InChamber)
                .Join(dbContext.ProductionBatches,
                      lot => lot.ProductionBatchId,
                      batch => batch.Id,
                      (lot, batch) => new { Lot = lot, Batch = batch })
                .Where(x => x.Batch.State == BatchState.InAging)
                .Select(x => new
                {
                    x.Lot.Id,
                    Name = string.IsNullOrWhiteSpace(x.Batch.Name)
                        ? x.Lot.BatchNumber
                        : $"{x.Batch.Name} ({x.Lot.BatchNumber})"
                })
                .ToListAsync(cancellationToken);

            return Results.Ok(activeLots);
        });

        group.MapGet(Endpoints.AGING_LOTS_ACTIVE_SUMMARY, async (GetActiveAgingLotsQueryHandler handler) =>
        {
            var result = await handler.HandleAsync();
            return Results.Ok(result);
        });

        group.MapGet($"{Endpoints.AGING_LOTS_ACTIVE}/{{id:guid}}", async (Guid id, GetAgingLotDetailsQueryHandler handler) =>
        {
            var result = await handler.HandleAsync(id);
            return result is not null ? Results.Ok(result) : Results.NotFound();
        });
    }
}