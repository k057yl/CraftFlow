using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Inventory.GetProductStockLots;
public class GetProductStockLotsHandler : IRequestHandler<GetProductStockLotsQuery, Result<List<ProductStockLotLookupDto>>>
{
    private readonly AppDbContext _dbContext;

    public GetProductStockLotsHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<ProductStockLotLookupDto>>> Handle(GetProductStockLotsQuery request, CancellationToken cancellationToken)
    {
        var productLots = await _dbContext.StockLots
            .AsNoTracking()
            .Join(_dbContext.Products,
                sl => sl.ItemId,
                p => p.Id,
                (sl, p) => new ProductStockLotLookupDto(
                    sl.Id,
                    p.Name + " (" + (sl.BatchNumber ?? "NoN") + " | In stock: " + sl.Quantity + " kg)"
                ))
            .ToListAsync(cancellationToken);

        return Result.Success(productLots);
    }
}