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

        var result = await (from l in query
                            join w in _dbContext.Warehouses.AsNoTracking() on l.WarehouseId equals w.Id into warehouses
                            from w in warehouses.DefaultIfEmpty()
                            join c in _dbContext.AgingChambers.AsNoTracking() on l.ChamberId equals c.Id into chambers
                            from c in chambers.DefaultIfEmpty()
                            select new StorageLocationDto(
                                l.Id,
                                l.Name,
                                l.LocationType,
                                l.WarehouseId,
                                w != null ? w.Name : null,
                                l.ChamberId,
                                c != null ? c.Name : null,
                                l.Capacity,
                                l.CurrentVolume,
                                l.IsOccupied,
                                l.BatchesProcessedCount,
                                l.WashCycleBatchInterval,
                                l.LastWashedAt,
                                l.BatchesProcessedCount >= l.WashCycleBatchInterval ? "WASH_REQUIRED" : "OK"
                            )).ToListAsync(cancellationToken);

        return Result.Success(result);
    }
}