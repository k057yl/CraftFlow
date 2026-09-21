using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Inventory.WriteOffStockLot;

public record WriteOffStockLotCommand(
    Guid StockLotId,
    decimal QuantityToWriteOff,
    string Reason
) : IRequest<Result<bool>>;