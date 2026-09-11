using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Inventory.DeleteWarehouse;

public class DeleteWarehouseHandler : IRequestHandler<DeleteWarehouseCommand, Result<bool>>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public DeleteWarehouseHandler(AppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<Result<bool>> Handle(DeleteWarehouseCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        var warehouse = await _dbContext.Warehouses
            .FirstOrDefaultAsync(w => w.Id == request.Id && w.TenantId == tenantId, cancellationToken);

        if (warehouse == null)
        {
            return Result.Failure<bool>(Error.NotFound(ErrorCodes.General.NOT_FOUND));
        }

        var hasStock = await _dbContext.StockLots
            .AnyAsync(s => s.WarehouseId == request.Id && s.Quantity > 0, cancellationToken);

        if (hasStock)
        {
            return Result.Failure<bool>(Error.Validation("WAREHOUSE_HAS_STOCK"));
        }

        _dbContext.Warehouses.Remove(warehouse);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}