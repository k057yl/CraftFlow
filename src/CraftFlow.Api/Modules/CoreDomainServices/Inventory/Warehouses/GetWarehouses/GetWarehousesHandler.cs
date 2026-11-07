using CraftFlow.Api.Core.Persistence;
using CraftFlow.SharedKernel.Dtos.Catalog.Inventory;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.CoreDomainServices.Inventory.Warehouses.GetWarehouses;

public class GetWarehousesHandler : IRequestHandler<GetWarehousesQuery, Result<List<WarehouseDto>>>
{
    private readonly AppDbContext _dbContext;

    public GetWarehousesHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<WarehouseDto>>> Handle(GetWarehousesQuery request, CancellationToken cancellationToken)
    {
        var warehouses = await _dbContext.Warehouses
            .AsNoTracking()
            .Select(w => new WarehouseDto(w.Id, w.Name, w.Address, w.Type))
            .ToListAsync(cancellationToken);

        return Result.Success(warehouses);
    }
}