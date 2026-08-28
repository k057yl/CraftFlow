using CraftFlow.Api.Common.Persistence;
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

            foreach (var ingredient in recipe.Ingredients)
            {
                var requiredQuantity = ingredient.Quantity * multiplier;

                var stockLots = await _dbContext.StockLots
                    .Where(s => s.WarehouseId == request.WarehouseId && s.ItemId == ingredient.RawMaterialId && s.Quantity > 0)
                    .ToListAsync(cancellationToken);

                var totalAvailable = stockLots.Sum(s => s.Quantity);

                if (totalAvailable < requiredQuantity)
                {
                    return Result.Failure<Guid>(Error.Validation(ErrorCodes.Production.INSUFFICIENT_RAW_MATERIAL));
                }

                var remainingToDeduct = requiredQuantity;
                foreach (var lot in stockLots)
                {
                    if (remainingToDeduct <= 0) break;

                    var deduct = Math.Min(lot.Quantity, remainingToDeduct);
                    lot.AdjustQuantity(-deduct);
                    remainingToDeduct -= deduct;
                }
            }

            var batch = ProductionBatch.Create(
                request.RecipeId,
                recipe.ProductId,
                request.WarehouseId,
                request.DestinationWarehouseId != Guid.Empty ? request.DestinationWarehouseId : request.WarehouseId,
                request.PlannedOutputQuantity
            );

            batch.Start();

            _dbContext.ProductionBatches.Add(batch);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(batch.Id);
        }
    }
}
