using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Inventory.WriteOffStockLot;

public class WriteOffStockLotHandler : IRequestHandler<WriteOffStockLotCommand, Result<bool>>
{
    private readonly AppDbContext _dbContext;

    public WriteOffStockLotHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<bool>> Handle(WriteOffStockLotCommand request, CancellationToken cancellationToken)
    {
        var lot = await _dbContext.StockLots
            .FirstOrDefaultAsync(l => l.Id == request.StockLotId, cancellationToken);

        if (lot == null)
        {
            return Result.Failure<bool>(Error.NotFound(ErrorCodes.Inventory.STOCK_LOT_NOT_FOUND));
        }

        lot.AdjustQuantity(-request.QuantityToWriteOff);

        // TODO: Регистрируем транзакцию списания/журнал потерь
        // _dbContext.StockAdjustments.Add(...);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}