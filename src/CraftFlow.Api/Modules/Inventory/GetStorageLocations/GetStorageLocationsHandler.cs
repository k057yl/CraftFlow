using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Dtos.Inventory;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Inventory.GetStorageLocations;

public class GetStorageLocationsHandler : IRequestHandler<GetStorageLocationsQuery, Result<List<StorageLocationDto>>>
{
    private readonly AppDbContext _dbContext;

    public GetStorageLocationsHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<StorageLocationDto>>> Handle(GetStorageLocationsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.StorageLocations.AsNoTracking();

        if (request.WarehouseId.HasValue)
        {
            query = query.Where(l => l.WarehouseId == request.WarehouseId.Value);
        }

        if (request.ChamberId.HasValue)
        {
            query = query.Where(l => l.ChamberId == request.ChamberId.Value);
        }

        var locations = await query
            .Select(l => new StorageLocationDto(
                l.Id,
                l.Name,
                l.LocationType,
                l.WarehouseId,
                l.ChamberId,
                l.Capacity,
                l.CurrentVolume,
                l.IsOccupied,
                l.BatchesProcessedCount,
                l.WashCycleBatchInterval,
                l.LastWashedAt,
                l.BatchesProcessedCount >= l.WashCycleBatchInterval ? "WASH_REQUIRED" : "OK"
            ))
            .ToListAsync(cancellationToken);

        var availableLocations = locations
            .Where(l => !l.Capacity.HasValue || l.FreeCapacity > 0)
            .ToList();

        return Result.Success(availableLocations);
    }
}