using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Inventory.Domain;
using CraftFlow.Api.Modules.Production.Domain;
using CraftFlow.SharedKernel.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Production.Events;

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
            .FirstOrDefaultAsync(r => r.Id == batch.RecipeId, cancellationToken);

        if (recipe != null && recipe.IsAgingRequired)
        {
            return;
        }

        var consumedIngredients = await _dbContext.Set<ConsumedIngredient>()
            .Where(ci => ci.ProductionBatchId == batch.Id)
            .ToListAsync(cancellationToken);

        decimal totalRawCost = 0m;

        foreach (var consumed in consumedIngredients)
        {
            var lot = await _dbContext.StockLots
                .AsNoTracking()
                .FirstOrDefaultAsync(sl => sl.Id == consumed.StockLotId, cancellationToken);

            if (lot != null)
            {
                totalRawCost += consumed.Quantity * lot.UnitPrice;
            }
        }

        decimal calculatedUnitCost = notification.ActualOutputQuantity > 0
            ? totalRawCost / notification.ActualOutputQuantity
            : 0m;

        var destWarehouseId = batch.DestinationWarehouseId != Guid.Empty
            ? batch.DestinationWarehouseId
            : batch.WarehouseId;

        var lotNumber = !string.IsNullOrWhiteSpace(batch.Name)
            ? batch.Name
            : string.Concat(FormattingConstants.BATCH_PREFIX, batch.Id.ToString()[..8].ToUpperInvariant());

        var finishedStockLot = StockLot.Create(
            warehouseId: destWarehouseId,
            itemId: batch.TargetProductId,
            initialQuantity: notification.ActualOutputQuantity,
            unitsCount: notification.UnitsCount,
            unitPrice: calculatedUnitCost,
            batchNumber: lotNumber,
            tenantId: batch.TenantId,
            productionBatchId: batch.Id
        );

        _dbContext.StockLots.Add(finishedStockLot);
    }
}