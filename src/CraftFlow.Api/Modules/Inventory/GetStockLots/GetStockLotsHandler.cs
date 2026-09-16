using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
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
        var rawMaterialIds = await _dbContext.RawMaterials
            .AsNoTracking()
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);

        var query = _dbContext.StockLots
            .AsNoTracking()
            .Where(x => x.Quantity > 0 && rawMaterialIds.Contains(x.ItemId));

        if (request.WarehouseId.HasValue && request.WarehouseId.Value != Guid.Empty)
        {
            query = query.Where(x => x.WarehouseId == request.WarehouseId.Value);
        }

        if (request.SupplierId.HasValue && request.SupplierId.Value != Guid.Empty)
        {
            query = query.Where(x => x.SupplierId == request.SupplierId.Value);
        }

        var now = DateTime.UtcNow;
        var warningThreshold = now.AddDays(7);

        if (request.OnlyExpired == true)
        {
            query = query.Where(x => x.ExpirationDate.HasValue && x.ExpirationDate.Value < now);
        }
        else if (request.OnlyExpiringSoon == true)
        {
            query = query.Where(x => x.ExpirationDate.HasValue && x.ExpirationDate.Value >= now && x.ExpirationDate.Value <= warningThreshold);
        }

        query = request.SortBy switch
        {
            UiConstants.SortConstants.EXPIRATION_ASC => query.OrderBy(x => x.ExpirationDate),
            UiConstants.SortConstants.EXPIRATION_DESC => query.OrderByDescending(x => x.ExpirationDate),
            UiConstants.SortConstants.QUANTITY_DESC => query.OrderByDescending(x => x.Quantity),
            UiConstants.SortConstants.SUPPLIER => query.OrderBy(x => x.SupplierId),
            _ => query.OrderByDescending(x => x.CreatedDate)
        };

        var rawItems = await query.ToListAsync(cancellationToken);

        var rawMaterialsMap = await _dbContext.RawMaterials.AsNoTracking().ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
        var warehousesMap = await _dbContext.Warehouses.AsNoTracking().ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
        var suppliersMap = await _dbContext.Suppliers.AsNoTracking().ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        var result = rawItems.Select(x => new StockLotDto(
            Id: x.Id,
            ItemName: rawMaterialsMap.GetValueOrDefault(x.ItemId, string.Empty),
            WarehouseName: warehousesMap.GetValueOrDefault(x.WarehouseId, string.Empty),
            SupplierName: x.SupplierId.HasValue ? suppliersMap.GetValueOrDefault(x.SupplierId.Value, string.Empty) : null,
            Quantity: x.Quantity,
            UnitPrice: x.UnitPrice,
            BatchNumber: x.BatchNumber,
            CreatedDate: x.CreatedDate,
            ExpirationDate: x.ExpirationDate,
            IsExpired: x.ExpirationDate.HasValue && x.ExpirationDate.Value < now,
            IsExpiringSoon: x.ExpirationDate.HasValue && x.ExpirationDate.Value >= now && x.ExpirationDate.Value <= warningThreshold
        )).ToList();

        return Result.Success(result);
    }
}