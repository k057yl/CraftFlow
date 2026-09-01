using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Aging.Domain;
using CraftFlow.Api.Modules.Aging.ReleaseFromAging;
using CraftFlow.Api.Modules.Aging.TransferToAging;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Aging;

public static class AgingEndpoints
{
    public static void MapAgingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/aging");

        group.MapPost("lots/transfer", async (TransferToAgingCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost("lots/release", async (ReleaseFromAgingCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
        });

        group.MapGet("chambers", async (AppDbContext dbContext, CancellationToken cancellationToken) =>
        {
            var chambers = await dbContext.AgingChambers
                .AsNoTracking()
                .Select(c => new { c.Id, Name = c.Name })
                .ToListAsync(cancellationToken);

            return Results.Ok(chambers);
        });

        group.MapGet("lots/active", async (AppDbContext dbContext, CancellationToken cancellationToken) =>
        {
            var activeLots = await dbContext.AgingLots
                .AsNoTracking()
                .Where(l => l.Status == AgingStatus.InChamber)
                .Join(dbContext.ProductionBatches,
                      lot => lot.ProductionBatchId,
                      batch => batch.Id,
                      (lot, batch) => new { Lot = lot, Batch = batch })
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
    }
}