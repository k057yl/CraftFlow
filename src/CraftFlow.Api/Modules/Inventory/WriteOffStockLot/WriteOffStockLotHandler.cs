using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Inventory.WriteOffStockLot;

public class WriteOffStockLotHandler : IRequestHandler<WriteOffStockLotCommand, Result<bool>>
{
    private readonly AppDbContext _dbContext;

    public WriteOffStockLotHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<bool>> Handle(WriteOffStockLotCommand request, CancellationToken cancellationToken)
    {
        if (request.QuantityToWriteOff <= 0)
        {
            return Result.Failure<bool>(Error.Validation(ErrorCodes.General.VALUE_REQUIRED));
        }

        var lot = await _dbContext.StockLots
            .Include(l => l.StorageLocations)
            .FirstOrDefaultAsync(l => l.Id == request.StockLotId, cancellationToken);

        if (lot == null)
        {
            return Result.Failure<bool>(Error.NotFound(ErrorCodes.Inventory.STOCK_LOT_NOT_FOUND));
        }

        if (request.QuantityToWriteOff > lot.Quantity)
        {
            return Result.Failure<bool>(Error.Validation(ErrorCodes.Inventory.STOCK_LOT_NEGATIVE_QUANTITY));
        }

        lot.AdjustQuantity(-request.QuantityToWriteOff);

        if (lot.StorageLocations.Count > 0)
        {
            var locationIds = lot.StorageLocations.Select(sl => sl.StorageLocationId).ToList();
            var locations = await _dbContext.StorageLocations
                .Where(sl => locationIds.Contains(sl.Id))
                .ToListAsync(cancellationToken);

            decimal remainingToWriteOff = request.QuantityToWriteOff;

            foreach (var stockLocation in lot.StorageLocations.ToList())
            {
                if (remainingToWriteOff <= 0) break;

                var location = locations.FirstOrDefault(l => l.Id == stockLocation.StorageLocationId);
                decimal amountToFree = Math.Min(stockLocation.AllocatedQuantity, remainingToWriteOff);

                if (location != null)
                {
                    location.AddVolume(-amountToFree);
                }

                remainingToWriteOff -= amountToFree;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}