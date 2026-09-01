using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Catalog.Domain;
using CraftFlow.Api.Modules.Inventory.Domain;
using CraftFlow.Api.Modules.Production.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Production.StartProductionBatch
{
    public class StartProductionBatchHandler : IRequestHandler<StartProductionBatchCommand, Result<Guid>>
    {
        private readonly AppDbContext _dbContext;

        public StartProductionBatchHandler(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<Guid>> Handle(StartProductionBatchCommand request, CancellationToken cancellationToken)
        {
            var recipe = await _dbContext.Recipes
                .Include(r => r.Ingredients)
                .FirstOrDefaultAsync(r => r.Id == request.RecipeId, cancellationToken);

            if (recipe == null)
            {
                return Result.Failure<Guid>(Error.NotFound(ErrorCodes.General.NOT_FOUND));
            }

            var multiplier = request.PlannedOutputQuantity / recipe.TargetOutputQuantity;

            var batchRequirements = new List<(RecipeIngredient Ingredient, decimal RequiredQuantity, List<StockLot> Lots)>();

            foreach (var recipeIngredient in recipe.Ingredients)
            {
                var requiredQuantity = recipeIngredient.Quantity * multiplier;

                var stockLots = await _dbContext.StockLots
                    .Where(s => s.WarehouseId == request.WarehouseId && s.ItemId == recipeIngredient.RawMaterialId && s.Quantity > 0)
                    .OrderBy(s => s.CreatedDate)
                    .ToListAsync(cancellationToken);

                var totalAvailable = stockLots.Sum(s => s.Quantity);

                if (totalAvailable < requiredQuantity)
                {
                    return Result.Failure<Guid>(Error.Validation(ErrorCodes.Production.INSUFFICIENT_RAW_MATERIAL));
                }

                batchRequirements.Add((recipeIngredient, requiredQuantity, stockLots));
            }

            var destinationWarehouse = request.DestinationWarehouseId != Guid.Empty
                ? request.DestinationWarehouseId
                : request.WarehouseId;

            var batch = ProductionBatch.Create(
                request.RecipeId,
                recipe.ProductId,
                request.WarehouseId,
                destinationWarehouse,
                request.PlannedOutputQuantity,
                request.Name
            );

            batch.Start();
            _dbContext.ProductionBatches.Add(batch);

            foreach (var requirement in batchRequirements)
            {
                var remainingToDeduct = requirement.RequiredQuantity;

                foreach (var lot in requirement.Lots)
                {
                    if (remainingToDeduct <= 0) break;

                    var deduct = Math.Min(lot.Quantity, remainingToDeduct);

                    lot.AdjustQuantity(-deduct);
                    remainingToDeduct -= deduct;

                    var consumed = ConsumedIngredient.Create(
                        batch.Id,
                        lot.Id,
                        requirement.Ingredient.RawMaterialId,
                        deduct,
                        batch.TenantId
                    );

                    await _dbContext.Set<ConsumedIngredient>().AddAsync(consumed, cancellationToken);
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(batch.Id);
        }
    }
}