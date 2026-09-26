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
        var now = DateTime.UtcNow;
        var soonThreshold = now.AddDays(7);

        var query = _dbContext.StockLots
            .AsNoTracking()
            .AsQueryable();

        if (request.WarehouseId.HasValue)
            query = query.Where(l => l.WarehouseId == request.WarehouseId.Value);

        if (request.SupplierId.HasValue)
            query = query.Where(l => l.SupplierId == request.SupplierId.Value);

        if (request.OnlyExpiringSoon == true)
            query = query.Where(l => l.ExpirationDate.HasValue && l.ExpirationDate.Value > now && l.ExpirationDate.Value <= soonThreshold);

        if (request.OnlyExpired == true)
            query = query.Where(l => l.ExpirationDate.HasValue && l.ExpirationDate.Value <= now);

        var result = await query
            .Select(lot => new StockLotDto(
                lot.Id,
                _dbContext.RawMaterials.Where(r => r.Id == lot.ItemId).Select(r => r.Name).FirstOrDefault()
                    ?? _dbContext.Products.Where(p => p.Id == lot.ItemId).Select(p => p.Name).FirstOrDefault()
                    ?? "—",
                _dbContext.Warehouses.Where(w => w.Id == lot.WarehouseId).Select(w => w.Name).FirstOrDefault() ?? "—",
                lot.SupplierId.HasValue
                    ? _dbContext.Suppliers.Where(s => s.Id == lot.SupplierId.Value).Select(s => s.Name).FirstOrDefault()
                    : null,
                lot.Quantity,
                lot.UnitPrice,
                lot.BatchNumber,
                lot.CreatedDate,
                lot.ExpirationDate,
                lot.ExpirationDate.HasValue && lot.ExpirationDate.Value <= now,
                lot.ExpirationDate.HasValue && lot.ExpirationDate.Value > now && lot.ExpirationDate.Value <= soonThreshold,
                string.Join(", ", lot.StorageLocations.Select(sl =>
                    _dbContext.StorageLocations.Where(loc => loc.Id == sl.StorageLocationId).Select(loc => loc.Name).FirstOrDefault() + " (" + sl.AllocatedQuantity + " л)"))
            ))
            .ToListAsync(cancellationToken);

        return Result.Success(result);
    }
}