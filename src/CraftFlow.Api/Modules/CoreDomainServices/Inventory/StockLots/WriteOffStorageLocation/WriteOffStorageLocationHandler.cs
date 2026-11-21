using CraftFlow.Api.Core.Persistence;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.CoreDomainServices.Inventory.StorageLocations.WriteOffStorageLocation;

public sealed class WriteOffStorageLocationHandler : IRequestHandler<WriteOffStorageLocationCommand, Result<bool>>
{
    private readonly AppDbContext _dbContext;

    public WriteOffStorageLocationHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<bool>> Handle(WriteOffStorageLocationCommand request, CancellationToken cancellationToken)
    {
        if (request.QuantityToWriteOff <= 0)
            return Result.Failure<bool>(Error.Validation(ErrorCodes.General.VALUE_REQUIRED));

        var location = await _dbContext.StorageLocations
            .FirstOrDefaultAsync(l => l.Id == request.StorageLocationId, cancellationToken);

        if (location is null)
            return Result.Failure<bool>(Error.NotFound(ErrorCodes.Inventory.LOCATION_NOT_FOUND));

        if (request.QuantityToWriteOff > location.CurrentVolume)
            return Result.Failure<bool>(Error.Validation(ErrorCodes.Inventory.STOCK_LOT_NEGATIVE_QUANTITY));

        var lotsInLocation = await _dbContext.StockLots
            .Include(l => l.StorageLocations)
            .Where(l => l.Quantity > 0 && l.StorageLocations.Any(sl => sl.StorageLocationId == request.StorageLocationId))
            .OrderBy(l => l.CreatedDate)
            .ToListAsync(cancellationToken);

        decimal remaining = request.QuantityToWriteOff;

        foreach (var lot in lotsInLocation)
        {
            if (remaining <= 0) break;

            var allocation = lot.StorageLocations.First(sl => sl.StorageLocationId == request.StorageLocationId);
            decimal deduct = Math.Min(Math.Min(lot.Quantity, allocation.AllocatedQuantity), remaining);

            if (deduct > 0)
            {
                lot.AdjustQuantity(-deduct);
                allocation.UpdateQuantity(allocation.AllocatedQuantity - deduct);
                remaining -= deduct;
            }
        }

        location.AddVolume(-request.QuantityToWriteOff);
        if (location.CurrentVolume <= 0.001m)
        {
            location.ClearVolume();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}