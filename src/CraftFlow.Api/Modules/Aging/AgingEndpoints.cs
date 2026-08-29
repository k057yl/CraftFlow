using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Aging.Domain;
using CraftFlow.Api.Modules.Aging.ReleaseFromAging;
using CraftFlow.Api.Modules.Aging.TransferToAging;
using CraftFlow.SharedKernel.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Aging;

public static class AgingEndpoints
{
    public static void MapAgingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost(Endpoints.AGING_LOTS_TRANSFER, async (TransferToAgingCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        app.MapPost(Endpoints.AGING_LOTS_RELEASE, async (ReleaseFromAgingCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
        });

        app.MapGet(Endpoints.AGING_LOTS_ACTIVE, async (AppDbContext dbContext, CancellationToken cancellationToken) =>
        {
            var activeLots = await dbContext.AgingLots
                .AsNoTracking()
                .Where(l => l.Status == AgingStatus.InChamber)
                .Select(l => new { l.Id, Name = l.BatchNumber })
                .ToListAsync(cancellationToken);

            return Results.Ok(activeLots);
        });
    }
}