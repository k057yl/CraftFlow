using CraftFlow.Api.Core.Extensions;
using CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Batches.CompleteCheeseBatch;
using CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Batches.DeleteDraftBatch;
using CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Batches.DiscardBatch;
using CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Batches.GetActiveBatchesSummary;
using CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Batches.GetBatchesReadyForAging;
using CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Batches.StartDraftBatch;
using CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Batches.StartProductionBatch;
using CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Costing.ConsumeIngredient;
using CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Costing.EstimateCost;
using CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Costing.GetBatchCost;
using CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Planning.CalculateMaxOutput;
using CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Planning.CalculateRequirements;
using CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Planning.PlanProductionBatch;
using CraftFlow.SharedKernel.Constants.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking;

public record CalculateRequirementsRequest(
    [FromQuery] Guid RecipeId,
    [FromQuery] Guid WarehouseId,
    [FromQuery] decimal PlannedQty);

public record EstimateCostRequest(
    [FromQuery] Guid RecipeId,
    [FromQuery] decimal PlannedQty);

public static class CheeseMakingEndpoints
{
    public static void MapCheeseMakingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(string.Empty)
            .WithTags("Cheese Making")
            .RequireAuthorization();

        group.MapGet(ProductionConstants.BATCHES_READY_AGING, async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetBatchesReadyForAgingQuery(), ct)).ToHttpResult());

        group.MapPost(ProductionConstants.BATCHES_START, async (StartCheeseBatchCommand command, ISender sender, CancellationToken ct) =>
            (await sender.Send(command, ct)).ToHttpResult());

        group.MapPost(ProductionConstants.BATCHES_COMPLETE, async (CompleteCheeseBatchCommand command, ISender sender, CancellationToken ct) =>
            (await sender.Send(command, ct)).ToHttpResult());

        group.MapPost(ProductionConstants.BATCHES_CONSUME, async (ConsumeIngredientCommand command, ISender sender, CancellationToken ct) =>
            (await sender.Send(command, ct)).ToHttpResult());

        group.MapPost(ProductionConstants.BATCHES_DISCARD, async (DiscardBatchCommand command, ISender sender, CancellationToken ct) =>
            (await sender.Send(command, ct)).ToHttpResult());

        group.MapGet(ProductionConstants.BATCHES_ACTIVE_SUMMARY, async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetActiveBatchesSummaryQuery(), ct)).ToHttpResult());

        group.MapGet($"{ProductionConstants.PRODUCTION_COSTING}/{{id:guid}}", async (Guid id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetBatchCostQuery(id), ct)).ToHttpResult());

        group.MapGet(ProductionConstants.CALCULATE_REQUIREMENTS, async ([AsParameters] CalculateRequirementsRequest request, ISender sender, CancellationToken ct) =>
            (await sender.Send(new CalculateBatchRequirementsQuery(request.RecipeId, request.WarehouseId, request.PlannedQty), ct)).ToHttpResult());

        group.MapGet(ProductionConstants.ESTIMATE_COST, async ([AsParameters] EstimateCostRequest request, ISender sender, CancellationToken ct) =>
            (await sender.Send(new EstimateCostQuery(request.RecipeId, request.PlannedQty), ct)).ToHttpResult());

        group.MapGet(ProductionConstants.CALCULATE_MAX_OUTPUT, async (Guid recipeId, Guid warehouseId, ISender sender, CancellationToken ct) =>
            (await sender.Send(new CalculateMaxOutputQuery(recipeId, warehouseId), ct)).ToHttpResult());

        group.MapPost(ProductionConstants.BATCHES_PLAN, async (PlanCheeseBatchCommand command, ISender sender, CancellationToken ct) =>
            (await sender.Send(command, ct)).ToHttpResult());

        group.MapDelete($"{ProductionConstants.BATCHES_DELETE_DRAFT}/{{id:guid}}", async (Guid id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new DeleteDraftBatchCommand(id), ct)).ToHttpResult());

        group.MapPost($"{ProductionConstants.BATCHES_START_DRAFT}/{{id:guid}}/start", async (Guid id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new StartDraftBatchCommand(id), ct)).ToHttpResult());
    }
}