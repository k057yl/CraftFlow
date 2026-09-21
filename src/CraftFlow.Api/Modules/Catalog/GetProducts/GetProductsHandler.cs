using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Catalog.GetProducts;

public class GetProductsHandler : IRequestHandler<GetProductsQuery, Result<List<ProductDto>>>
{
    private readonly AppDbContext _dbContext;

    public GetProductsHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _dbContext.Products
            .AsNoTracking()
            .Join(
                _dbContext.UnitsOfMeasure.AsNoTracking(),
                prod => prod.UnitOfMeasureId,
                uom => uom.Id,
                (prod, uom) => new ProductDto(prod.Id, prod.Name, prod.UnitOfMeasureId, uom.Code)
            )
            .ToListAsync(cancellationToken);

        return Result.Success(products);
    }
}
