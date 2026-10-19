using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Dtos.Inventory;
using CraftFlow.SharedKernel.Enums.Sanitation;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Inventory.StorageLocations.GetStorageLocations;

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

        var rawLocations = await (from l in query
                                  join w in _dbContext.Warehouses.AsNoTracking() on l.WarehouseId equals w.Id into warehouses
                                  from w in warehouses.DefaultIfEmpty()
                                  join c in _dbContext.AgingChambers.AsNoTracking() on l.ChamberId equals c.Id into chambers
                                  from c in chambers.DefaultIfEmpty()
                                  select new
                                  {
                                      Location = l,
                                      WarehouseName = w != null ? w.Name : null,
                                      ChamberName = c != null ? c.Name : null
                                  }).ToListAsync(cancellationToken);

        var locationIds = rawLocations.Select(x => x.Location.Id).ToList();

        var rules = await _dbContext.SanitationRules.AsNoTracking()
            .Where(r => r.TargetType == SanitationTargetType.StorageLocation && locationIds.Contains(r.TargetId))
            .ToDictionaryAsync(r => r.TargetId, cancellationToken);

        var result = rawLocations.Select(x =>
        {
            var l = x.Location;
            var nextWashDueDate = l.NextWashDueDate;

            if (!nextWashDueDate.HasValue && l.LastWashedAt.HasValue && rules.TryGetValue(l.Id, out var rule) && rule.IntervalDays.HasValue)
            {
                nextWashDueDate = l.LastWashedAt.Value.AddDays(rule.IntervalDays.Value);
            }

            return new StorageLocationDto(
                l.Id,
                l.Name,
                l.LocationType,
                l.WarehouseId,
                x.WarehouseName,
                l.ChamberId,
                x.ChamberName,
                l.Capacity,
                l.CurrentVolume,
                l.IsOccupied,
                l.BatchesProcessedCount,
                l.WashCycleBatchInterval,
                l.LastWashedAt,
                nextWashDueDate,
                l.BatchesProcessedCount >= l.WashCycleBatchInterval ? "WASH_REQUIRED" : "OK",
                l.UnitOfMeasure
            );
        }).ToList();

        return Result.Success(result);
    }
}