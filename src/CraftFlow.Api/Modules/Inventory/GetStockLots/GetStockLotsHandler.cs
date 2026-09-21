using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Inventory.GetStockLots;

public class GetStockLotsHandler : IRequestHandler<GetStockLotsQuery, Result<List<StockLotDto>>>
{
    private readonly AppDbContext _dbContext;

    public GetStockLotsHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<StockLotDto>>> Handle(GetStockLotsQuery request, CancellationToken cancellationToken)
    {
        var lots = await _dbContext.StockLots
            .AsNoTracking()
            .Include(sl => sl.StorageLocations)
            .ToListAsync(cancellationToken);

        var rawMaterialsMap = await _dbContext.RawMaterials.AsNoTracking().ToDictionaryAsync(r => r.Id, r => r.Name, cancellationToken);
        var productsMap = await _dbContext.Products.AsNoTracking().ToDictionaryAsync(p => p.Id, p => p.Name, cancellationToken);
        var warehousesMap = await _dbContext.Warehouses.AsNoTracking().ToDictionaryAsync(w => w.Id, w => w.Name, cancellationToken);
        var suppliersMap = await _dbContext.Suppliers.AsNoTracking().ToDictionaryAsync(s => s.Id, s => s.Name, cancellationToken);
        var storageLocationsMap = await _dbContext.StorageLocations.AsNoTracking().ToDictionaryAsync(sl => sl.Id, sl => sl.Name, cancellationToken);

        var now = DateTime.UtcNow;
        var soonThreshold = now.AddDays(7);

        var result = new List<StockLotDto>();

        foreach (var lot in lots)
        {
            string itemName = rawMaterialsMap.TryGetValue(lot.ItemId, out var rmName)
                ? rmName
                : (productsMap.TryGetValue(lot.ItemId, out var pName) ? pName : "—");

            string warehouseName = warehousesMap.TryGetValue(lot.WarehouseId, out var wName) ? wName : "—";
            string? supplierName = lot.SupplierId.HasValue && suppliersMap.TryGetValue(lot.SupplierId.Value, out var sName) ? sName : null;

            var isExpired = lot.ExpirationDate.HasValue && lot.ExpirationDate.Value <= now;
            var isExpiringSoon = lot.ExpirationDate.HasValue && !isExpired && lot.ExpirationDate.Value <= soonThreshold;
            var locInfoList = lot.StorageLocations
                .Where(sl => storageLocationsMap.ContainsKey(sl.StorageLocationId))
                .Select(sl => $"{storageLocationsMap[sl.StorageLocationId]} ({sl.AllocatedQuantity:N0} л)")
                .ToList();

            string? locationsInfo = locInfoList.Count > 0 ? string.Join(", ", locInfoList) : "—";

            result.Add(new StockLotDto(
                lot.Id,
                itemName,
                warehouseName,
                supplierName,
                lot.Quantity,
                lot.UnitPrice,
                lot.BatchNumber,
                lot.CreatedDate,
                lot.ExpirationDate,
                isExpired,
                isExpiringSoon,
                locationsInfo
            ));
        }

        if (request.WarehouseId.HasValue)
            result = result.Where(r => lots.Any(l => l.Id == r.Id && l.WarehouseId == request.WarehouseId.Value)).ToList();

        if (request.SupplierId.HasValue)
            result = result.Where(r => lots.Any(l => l.Id == r.Id && l.SupplierId == request.SupplierId.Value)).ToList();

        if (request.OnlyExpiringSoon == true)
            result = result.Where(r => r.IsExpiringSoon).ToList();

        if (request.OnlyExpired == true)
            result = result.Where(r => r.IsExpired).ToList();

        return Result.Success(result);
    }
}