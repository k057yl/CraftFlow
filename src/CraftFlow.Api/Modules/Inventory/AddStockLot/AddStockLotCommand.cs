using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Inventory.AddStockLot;

public record AddStockLotCommand(
    Guid WarehouseId,
    Guid ItemId,
    decimal Quantity,
    int UnitsCount,
    decimal UnitPrice,
    string? BatchNumber
) : IRequest<Result<Guid>>;