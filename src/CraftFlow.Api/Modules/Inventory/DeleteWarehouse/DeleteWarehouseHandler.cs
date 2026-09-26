using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Inventory.DeleteWarehouse;

public class DeleteWarehouseHandler : IRequestHandler<DeleteWarehouseCommand, Result<bool>>
{
    private readonly AppDbContext _dbContext;

    public DeleteWarehouseHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<bool>> Handle(DeleteWarehouseCommand request, CancellationToken cancellationToken)
    {
        var hasStock = await _dbContext.StockLots
            .AnyAsync(s => s.WarehouseId == request.Id && s.Quantity > 0, cancellationToken);

        if (hasStock)
        {
            return Result.Failure<bool>(Error.Validation("WAREHOUSE_HAS_STOCK"));
        }

        var rowsAffected = await _dbContext.Warehouses
            .Where(w => w.Id == request.Id)
            .ExecuteDeleteAsync(cancellationToken);

        if (rowsAffected == 0)
        {
            return Result.Failure<bool>(Error.NotFound(ErrorCodes.General.NOT_FOUND));
        }

        return Result.Success(true);
    }
}