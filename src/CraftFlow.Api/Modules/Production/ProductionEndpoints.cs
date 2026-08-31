using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Production.CompleteProductionBatch;
using CraftFlow.Api.Modules.Production.ConsumeIngredient;
using CraftFlow.Api.Modules.Production.Domain;
using CraftFlow.Api.Modules.Production.GetActiveBatches;
using CraftFlow.Api.Modules.Production.GetBatchCost;
using CraftFlow.Api.Modules.Production.StartProductionBatch;
using CraftFlow.SharedKernel.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Production;

public static class ProductionEndpoints
{
    public static void MapProductionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("")
            .WithTags("Production");

        group.MapPost(Endpoints.BATCHES_START, async (StartProductionBatchCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost(Endpoints.BATCHES_COMPLETE, async (CompleteProductionBatchCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost(Endpoints.BATCHES_CONSUME, async (ConsumeIngredientCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost(Endpoints.BATCHES_DISCARD, async (DiscardBatchRequest request, AppDbContext dbContext, CancellationToken cancellationToken) =>
        {
            var batch = await dbContext.ProductionBatches
                .FirstOrDefaultAsync(b => b.Id == request.BatchId, cancellationToken);

            if (batch is null) return Results.NotFound();

            batch.Discard(request.Reason);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.Ok();
        });

        group.MapGet(Endpoints.BATCHES_ACTIVE, async (ISender sender) =>
        {
            var result = await sender.Send(new GetActiveBatchesQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet(Endpoints.BATCHES_READY_AGING, async (AppDbContext dbContext, CancellationToken cancellationToken) =>
        {
            var batches = await dbContext.ProductionBatches
                .AsNoTracking()
                .Where(b => b.Status == BatchStatus.Completed)
                .Join(dbContext.Recipes,
                      batch => batch.RecipeId,
                      recipe => recipe.Id,
                      (batch, recipe) => new { Batch = batch, Recipe = recipe })
                .Where(br => br.Recipe.IsAgingRequired)
                .Select(br => new
                {
                    Id = br.Batch.Id,
                    Name = $"Партия ГП #{br.Batch.Id.ToString().Substring(0, 8)}",
                    DefaultAgingDays = br.Recipe.DefaultMinAgingDays ?? 0
                })
                .ToListAsync(cancellationToken);

            return Results.Ok(batches);
        });

        group.MapGet($"{Endpoints.PRODUCTION_COSTING}/{{id:guid}}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetBatchCostQuery(id));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet(Endpoints.CALCULATE_REQUIREMENTS, async (Guid recipeId, Guid warehouseId, decimal plannedQty, AppDbContext dbContext) =>
        {
            var recipe = await dbContext.Recipes
                .Include(r => r.Ingredients)
                .FirstOrDefaultAsync(r => r.Id == recipeId);

            if (recipe == null || recipe.TargetOutputQuantity <= 0)
                return Results.Ok(new List<object>());

            var rawMaterialIds = recipe.Ingredients.Select(i => i.RawMaterialId).ToList();
            var rawMaterials = await dbContext.RawMaterials
                .Where(rm => rawMaterialIds.Contains(rm.Id))
                .ToDictionaryAsync(rm => rm.Id, rm => rm.Name);

            var multiplier = plannedQty / recipe.TargetOutputQuantity;
            var result = new List<object>();

            foreach (var ingredient in recipe.Ingredients)
            {
                var requiredQty = ingredient.Quantity * multiplier;

                var availableQty = await dbContext.StockLots
                    .Where(s => s.WarehouseId == warehouseId && s.ItemId == ingredient.RawMaterialId)
                    .SumAsync(s => s.Quantity);

                var matName = rawMaterials.TryGetValue(ingredient.RawMaterialId, out var name) ? name : "Сырье";

                result.Add(new
                {
                    MaterialName = matName,
                    RequiredQty = requiredQty,
                    AvailableQty = availableQty,
                    IsSufficient = availableQty >= requiredQty
                });
            }

            return Results.Ok(result);
        });

        group.MapGet(Endpoints.ESTIMATE_COST, async (Guid recipeId, decimal plannedQty, AppDbContext dbContext) =>
        {
            var recipe = await dbContext.Recipes
                .Include(r => r.Ingredients)
                .FirstOrDefaultAsync(r => r.Id == recipeId);

            if (recipe == null || recipe.TargetOutputQuantity <= 0)
                return Results.Ok(0m);

            var multiplier = plannedQty / recipe.TargetOutputQuantity;
            decimal totalEstimatedCost = 0m;

            foreach (var ingredient in recipe.Ingredients)
            {
                var requiredQty = ingredient.Quantity * multiplier;

                var activeLots = await dbContext.StockLots
                    .Where(s => s.ItemId == ingredient.RawMaterialId && s.Quantity > 0)
                    .Select(s => new { s.Quantity, s.UnitPrice })
                    .ToListAsync();

                var totalQuantityOnStock = activeLots.Sum(l => l.Quantity);

                decimal weightedAvgUnitPrice = 0m;

                if (totalQuantityOnStock > 0)
                {
                    var totalStockCost = activeLots.Sum(l => l.Quantity * l.UnitPrice);
                    weightedAvgUnitPrice = totalStockCost / totalQuantityOnStock;
                }

                totalEstimatedCost += requiredQty * weightedAvgUnitPrice;
            }

            return Results.Ok(totalEstimatedCost);
        });
    }
}
public record DiscardBatchRequest(Guid BatchId, string Reason);