using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Inventory.Domain;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Inventory.AddStockLot;

public class AddStockLotHandler : IRequestHandler<AddStockLotCommand, Result<Guid>>
{
    private readonly AppDbContext _dbContext;

    public AddStockLotHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(AddStockLotCommand request, CancellationToken cancellationToken)
    {
        DateTime? utcExpirationDate = request.ExpirationDate.HasValue
            ? DateTime.SpecifyKind(request.ExpirationDate.Value, DateTimeKind.Utc)
            : null;

        int finalUnitsCount = request.UnitsCount ?? 1;

        var stockLot = StockLot.Create(
            warehouseId: request.WarehouseId,
            itemId: request.ItemId,
            initialQuantity: request.Quantity,
            unitsCount: finalUnitsCount,
            unitPrice: request.UnitPrice,
            batchNumber: request.BatchNumber,
            supplierId: request.SupplierId,
            expirationDate: utcExpirationDate
        );

        if (request.StorageLocationIds != null && request.StorageLocationIds.Count > 0)
        {
            var locations = await _dbContext.StorageLocations
                .Where(l => request.StorageLocationIds.Contains(l.Id))
                .ToListAsync(cancellationToken);

            decimal remainingQuantity = request.Quantity;

            foreach (var location in locations)
            {
                if (remainingQuantity <= 0) break;

                decimal freeCapacity = location.Capacity.HasValue
                    ? Math.Max(0, location.Capacity.Value - location.CurrentVolume)
                    : remainingQuantity;

                decimal fillAmount = Math.Min(remainingQuantity, freeCapacity);

                if (fillAmount > 0)
                {
                    location.AddVolume(fillAmount);
                    location.RegisterBatchProcessed();
                    stockLot.AssignStorageLocation(location.Id, fillAmount);
                    remainingQuantity -= fillAmount;
                }
            }
        }

        _dbContext.StockLots.Add(stockLot);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(stockLot.Id);
    }
}