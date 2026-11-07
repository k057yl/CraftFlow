using CraftFlow.Api.Core.Persistence;
using CraftFlow.SharedKernel.Dtos.Catalog.Inventory;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.CoreDomainServices.Inventory.StockLots.GetProductStockLots;
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
            .Where(sl => sl.Quantity > 0)
            .Join(_dbContext.Products.IgnoreQueryFilters(),
                sl => sl.ItemId,
                p => p.Id,
                (sl, p) => new ProductStockLotLookupDto(
                    sl.Id,
                    $"{p.Name} (Партия: {sl.BatchNumber ?? "б/н"} | Остаток: {sl.Quantity:0.##})"
                ))
            .ToListAsync(cancellationToken);

        return Result.Success(productLots);
    }
}