using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Dtos.Inventory;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Inventory.StockLots.GetRawStockLots;
public class GetRawStockLotsHandler : IRequestHandler<GetRawStockLotsQuery, Result<List<RawStockLotLookupDto>>>
{
    private readonly AppDbContext _dbContext;

    public GetRawStockLotsHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<RawStockLotLookupDto>>> Handle(GetRawStockLotsQuery request, CancellationToken cancellationToken)
    {
        var rawLots = await _dbContext.StockLots
            .AsNoTracking()
            .Where(sl => sl.Quantity > 0)
            .Join(_dbContext.RawMaterials.IgnoreQueryFilters(),
                sl => sl.ItemId,
                rm => rm.Id,
                (sl, rm) => new RawStockLotLookupDto(
                    sl.Id,
                    $"{rm.Name} (Партия: {sl.BatchNumber ?? "б/н"} | Остаток: {sl.Quantity:0.##})"
                ))
            .ToListAsync(cancellationToken);

        return Result.Success(rawLots);
    }
}