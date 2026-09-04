using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Production.CompleteProductionBatch;
using CraftFlow.Api.Modules.Production.ConsumeIngredient;
using CraftFlow.Api.Modules.Production.DiscardBatch;
using CraftFlow.Api.Modules.Production.Domain;
using CraftFlow.Api.Modules.Production.GetActiveBatchesSummary;
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
            .WithTags("Production")
            .RequireAuthorization();

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

        group.MapPost(Endpoints.BATCHES_DISCARD, async (DiscardBatchCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
        });

        group.MapGet(Endpoints.BATCHES_ACTIVE_SUMMARY, async (ISender sender) =>
        {
            var result = await sender.Send(new GetActiveBatchesSummaryQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet(Endpoints.BATCHES_READY_AGING, async (AppDbContext dbContext, CancellationToken cancellationToken) =>
        {
            var existingAgingBatchIds = await dbContext.AgingLots
                .AsNoTracking()
                .Select(l => l.ProductionBatchId)
                .ToListAsync(cancellationToken);

            var batches = await dbContext.ProductionBatches
                .AsNoTracking()
                .Where(b => (b.State == BatchState.Completed || b.State == BatchState.ReadyForAging)
                         && !existingAgingBatchIds.Contains(b.Id))
                .Join(dbContext.Recipes,
                      batch => batch.RecipeId,
                      recipe => recipe.Id,
                      (batch, recipe) => new { Batch = batch, Recipe = recipe })
                .Where(br => br.Recipe.IsAgingRequired)
                .Select(br => new
                {
                    Id = br.Batch.Id,
                    Name = string.IsNullOrWhiteSpace(br.Batch.Name)
                        ? $"Party #{br.Batch.Id.ToString().Substring(0, 8)} (Exit: {(br.Batch.ActualOutputQuantity > 0 ? br.Batch.ActualOutputQuantity : br.Batch.PlannedOutputQuantity)} кг)"
                        : $"{br.Batch.Name} (Exit: {(br.Batch.ActualOutputQuantity > 0 ? br.Batch.ActualOutputQuantity : br.Batch.PlannedOutputQuantity)} kg)",
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
                .AsNoTracking()
                .Include(r => r.Ingredients)
                .FirstOrDefaultAsync(r => r.Id == recipeId);

            if (recipe == null || recipe.TargetOutputQuantity <= 0 || recipe.Ingredients == null || !recipe.Ingredients.Any())
                return Results.Ok(new List<object>());

            var rawMaterialIds = recipe.Ingredients.Select(i => i.RawMaterialId).ToList();
            var rawMaterials = await dbContext.RawMaterials
                .AsNoTracking()
                .Where(rm => rawMaterialIds.Contains(rm.Id))
                .ToDictionaryAsync(rm => rm.Id, rm => rm.Name);

            var multiplier = plannedQty / recipe.TargetOutputQuantity;
            var result = new List<object>();

            foreach (var ingredient in recipe.Ingredients)
            {
                var requiredQty = Math.Round(ingredient.Quantity * multiplier, 3);

                var availableQty = await dbContext.StockLots
                    .AsNoTracking()
                    .Where(s => s.WarehouseId == warehouseId && s.ItemId == ingredient.RawMaterialId && s.Quantity > 0)
                    .SumAsync(s => (decimal?)s.Quantity) ?? 0m;

                var matName = rawMaterials.TryGetValue(ingredient.RawMaterialId, out var name) ? name : "Raw materials";
                var roundedAvailable = Math.Round(availableQty, 3);
                var isSufficient = (roundedAvailable + 0.001m) >= requiredQty;

                result.Add(new
                {
                    MaterialName = matName,
                    RequiredQty = requiredQty,
                    AvailableQty = roundedAvailable,
                    IsSufficient = isSufficient
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

        group.MapGet(Endpoints.CALCULATE_MAX_OUTPUT, async (Guid recipeId, Guid warehouseId, AppDbContext dbContext) =>
        {
            var recipe = await dbContext.Recipes
                .Include(r => r.Ingredients)
                .FirstOrDefaultAsync(r => r.Id == recipeId);

            if (recipe == null || recipe.TargetOutputQuantity <= 0 || !recipe.Ingredients.Any())
                return Results.Ok(0m);

            decimal maxPossibleMultiplier = decimal.MaxValue;

            foreach (var ingredient in recipe.Ingredients)
            {
                if (ingredient.Quantity <= 0) continue;

                var availableStock = await dbContext.StockLots
                    .Where(s => s.WarehouseId == warehouseId && s.ItemId == ingredient.RawMaterialId && s.Quantity > 0)
                    .SumAsync(s => s.Quantity);

                if (availableStock <= 0)
                {
                    return Results.Ok(0m);
                }

                var ingredientLimitMultiplier = availableStock / ingredient.Quantity;

                if (ingredientLimitMultiplier < maxPossibleMultiplier)
                {
                    maxPossibleMultiplier = ingredientLimitMultiplier;
                }
            }

            if (maxPossibleMultiplier == decimal.MaxValue || maxPossibleMultiplier <= 0)
                return Results.Ok(0m);

            var maxPlannedOutput = Math.Floor(recipe.TargetOutputQuantity * maxPossibleMultiplier * 100m) / 100m;

            return Results.Ok(maxPlannedOutput);
        });
    }
}