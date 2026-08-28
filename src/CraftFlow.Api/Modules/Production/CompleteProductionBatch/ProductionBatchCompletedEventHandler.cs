using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Inventory.Domain;
using CraftFlow.Api.Modules.Production.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Production.CompleteProductionBatch;

public class ProductionBatchCompletedEventHandler : INotificationHandler<ProductionBatchCompletedEvent>
{
    private readonly AppDbContext _dbContext;

    public ProductionBatchCompletedEventHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(ProductionBatchCompletedEvent notification, CancellationToken cancellationToken)
    {
        var batch = await _dbContext.ProductionBatches
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == notification.BatchId, cancellationToken);

        if (batch == null) return;

        var recipe = await _dbContext.Recipes
            .AsNoTracking()
            .Include(r => r.Ingredients)
            .FirstOrDefaultAsync(r => r.Id == batch.RecipeId, cancellationToken);

        decimal calculatedUnitCost = 0m;

        if (recipe != null && recipe.TargetOutputQuantity > 0 && notification.ActualOutputQuantity > 0)
        {
            var planMultiplier = batch.PlannedOutputQuantity / recipe.TargetOutputQuantity;
            decimal totalRawCost = 0m;

            foreach (var ingredient in recipe.Ingredients)
            {
                var requiredQty = ingredient.Quantity * planMultiplier;
                var avgPrice = await _dbContext.StockLots
                    .Where(s => s.ItemId == ingredient.RawMaterialId)
                    .Select(s => (decimal?)s.UnitPrice)
                    .AverageAsync(cancellationToken) ?? 0m;

                totalRawCost += requiredQty * avgPrice;
            }

            calculatedUnitCost = totalRawCost / notification.ActualOutputQuantity;
        }

        var destWarehouseId = batch.DestinationWarehouseId != Guid.Empty
            ? batch.DestinationWarehouseId
            : batch.WarehouseId;

        var finishedStockLot = StockLot.Create(
            destWarehouseId,
            batch.TargetProductId,
            notification.ActualOutputQuantity,
            calculatedUnitCost,
            $"BATCH-{batch.Id.ToString()[..8].ToUpper()}"
        );

        _dbContext.StockLots.Add(finishedStockLot);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}