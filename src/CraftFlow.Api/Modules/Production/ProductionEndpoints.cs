using CraftFlow.Api.Modules.Production.Batches.CompleteProductionBatch;
using CraftFlow.Api.Modules.Production.Batches.DeleteDraftBatch;
using CraftFlow.Api.Modules.Production.Batches.DiscardBatch;
using CraftFlow.Api.Modules.Production.Batches.GetActiveBatchesSummary;
using CraftFlow.Api.Modules.Production.Batches.GetBatchesReadyForAging;
using CraftFlow.Api.Modules.Production.Batches.StartDraftBatch;
using CraftFlow.Api.Modules.Production.Batches.StartProductionBatch;
using CraftFlow.Api.Modules.Production.Costing.ConsumeIngredient;
using CraftFlow.Api.Modules.Production.Costing.EstimateCost;
using CraftFlow.Api.Modules.Production.Costing.GetBatchCost;
using CraftFlow.Api.Modules.Production.Planning.CalculateMaxOutput;
using CraftFlow.Api.Modules.Production.Planning.CalculateRequirements;
using CraftFlow.Api.Modules.Production.Planning.PlanProductionBatch;
using MediatR;

namespace CraftFlow.Api.Modules.Production;

public static class ProductionEndpoints
{
    public static void MapProductionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(string.Empty)
            .WithTags("Production")
            .RequireAuthorization();

        group.MapPost(ProductionConstants.BATCHES_START, async (StartProductionBatchCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost(ProductionConstants.BATCHES_COMPLETE, async (CompleteProductionBatchCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost(ProductionConstants.BATCHES_CONSUME, async (ConsumeIngredientCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost(ProductionConstants.BATCHES_DISCARD, async (DiscardBatchCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
        });

        group.MapGet(ProductionConstants.BATCHES_ACTIVE_SUMMARY, async (ISender sender) =>
        {
            var result = await sender.Send(new GetActiveBatchesSummaryQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet(ProductionConstants.BATCHES_READY_AGING, async (ISender sender) =>
        {
            var result = await sender.Send(new GetBatchesReadyForAgingQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet($"{ProductionConstants.PRODUCTION_COSTING}/{{id:guid}}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetBatchCostQuery(id));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet(ProductionConstants.CALCULATE_REQUIREMENTS, async (Guid recipeId, Guid warehouseId, decimal plannedQty, ISender sender) =>
        {
            var result = await sender.Send(new CalculateBatchRequirementsQuery(recipeId, warehouseId, plannedQty));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet(ProductionConstants.ESTIMATE_COST, async (Guid recipeId, decimal plannedQty, ISender sender) =>
        {
            var result = await sender.Send(new EstimateCostQuery(recipeId, plannedQty));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet(ProductionConstants.CALCULATE_MAX_OUTPUT, async (Guid recipeId, Guid warehouseId, ISender sender) =>
        {
            var result = await sender.Send(new CalculateMaxOutputQuery(recipeId, warehouseId));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost(ProductionConstants.BATCHES_PLAN, async (PlanProductionBatchCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapDelete($"{ProductionConstants.BATCHES_DELETE_DRAFT}/{{id:guid}}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteDraftBatchCommand(id));
            return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
        });

        group.MapPost($"{ProductionConstants.BATCHES_START_DRAFT}/{{id:guid}}/start", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new StartDraftBatchCommand(id));
            return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
        });
    }
}