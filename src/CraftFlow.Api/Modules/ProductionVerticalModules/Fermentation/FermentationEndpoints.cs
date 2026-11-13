using CraftFlow.Api.Modules.ProductionVerticalModules.Fermentation.AddFermentationReading;
using CraftFlow.Api.Modules.ProductionVerticalModules.Fermentation.ChangeStage;
using CraftFlow.Api.Modules.ProductionVerticalModules.Fermentation.CompleteFermentationBatch;
using CraftFlow.Api.Modules.ProductionVerticalModules.Fermentation.CreateFermentationBatch;
using CraftFlow.Api.Modules.ProductionVerticalModules.Fermentation.GetFermentationBatches;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Constants.Endpoints;
using CraftFlow.SharedKernel.Dtos.Fermentation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.Fermentation;

public static class FermentationEndpoints
{
    public static void MapFermentationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("")
            .WithTags("Fermentation")
            .RequireAuthorization();

        group.MapGet(FermentationConstants.BATCHES, async ([FromQuery] bool includeCompleted, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetFermentationBatchesQuery(includeCompleted), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost(FermentationConstants.BATCHES, async ([FromBody] CreateFermentationBatchCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : (result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : Results.BadRequest(result.Error));
        });

        group.MapPost($"{FermentationConstants.BATCHES}/{{id:guid}}/{FermentationConstants.READINGS}", async (
            [FromRoute] Guid id,
            [FromBody] AddFermentationReadingRequest body,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new AddFermentationReadingCommand(
                id,
                body.Ph,
                body.Temperature,
                body.DegassedOrBurped,
                body.BrineToppedUp,
                body.SensoryNotes);

            var result = await sender.Send(command, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : (result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : Results.BadRequest(result.Error));
        });

        group.MapPost($"{FermentationConstants.BATCHES}/{{id:guid}}/{FermentationConstants.MOVE_TO_COLD}", async (
            [FromRoute] Guid id,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new MoveToColdStorageCommand(id), ct);
            return result.IsSuccess
                ? Results.NoContent()
                : (result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : Results.BadRequest(result.Error));
        });

        group.MapPost($"{FermentationConstants.BATCHES}/{{id:guid}}/{FermentationConstants.COMPLETE}", async (
            [FromRoute] Guid id,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CompleteFermentationBatchCommand(id), ct);
            return result.IsSuccess
                ? Results.NoContent()
                : (result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : Results.BadRequest(result.Error));
        });
    }
}