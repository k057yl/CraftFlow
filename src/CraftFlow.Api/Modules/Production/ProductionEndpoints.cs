using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Production.CompleteProductionBatch;
using CraftFlow.Api.Modules.Production.GetActiveBatches;
using CraftFlow.Api.Modules.Production.GetBatchCost;
using CraftFlow.Api.Modules.Production.StartProductionBatch;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Production
{
    public static class ProductionEndpoints
    {
        public static void MapProductionEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/production")
                .WithTags("Production");

            group.MapPost("/batches/start", async (StartProductionBatchCommand command, ISender sender) =>
            {
                var result = await sender.Send(command);
                return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
            });

            group.MapPost("/batches/complete", async (CompleteProductionBatchCommand command, ISender sender) =>
            {
                var result = await sender.Send(command);
                return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
            });

            group.MapGet("/batches/active", async (ISender sender) =>
            {
                var result = await sender.Send(new GetActiveBatchesQuery());
                return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
            });

            group.MapGet("/costing/{id:guid}", async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new GetBatchCostQuery(id));
                return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
            });

            group.MapGet("/calculate-requirements", async (Guid recipeId, Guid warehouseId, decimal plannedQty, AppDbContext dbContext) =>
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

            // --- Расчет средневзвешенной стоимости партии ---
            group.MapGet("/estimate-cost", async (Guid recipeId, decimal plannedQty, AppDbContext dbContext) =>
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
}