using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Inventory.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Inventory.AddStockLot;

public class AddStockLotHandler : IRequestHandler<AddStockLotCommand, Result<Guid>>
{
    private readonly AppDbContext _dbContext;

    public AddStockLotHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(AddStockLotCommand request, CancellationToken cancellationToken)
    {
        var warehouseExists = await _dbContext.Warehouses
            .AnyAsync(w => w.Id == request.WarehouseId, cancellationToken);

        if (!warehouseExists)
        {
            return Result.Failure<Guid>(Error.NotFound(ErrorCodes.Inventory.WAREHOUSE_NOT_FOUND));
        }

        DateTime? utcExpirationDate = request.ExpirationDate.HasValue
            ? DateTime.SpecifyKind(request.ExpirationDate.Value, DateTimeKind.Utc)
            : null;

        int finalUnitsCount = request.UnitsCount ?? 1;

        var stockLot = StockLot.Create(
            warehouseId: request.WarehouseId,
            itemId: request.ItemId,
            initialQuantity: request.Quantity,
            unitsCount: finalUnitsCount,
            unitPrice: request.UnitPrice,
            batchNumber: request.BatchNumber,
            supplierId: request.SupplierId,
            expirationDate: utcExpirationDate
        );

        _dbContext.StockLots.Add(stockLot);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(stockLot.Id);
    }
}