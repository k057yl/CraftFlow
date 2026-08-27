using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Inventory.AddStockLot
{
    public record AddStockLotCommand(
        Guid WarehouseId,
        Guid ItemId,
        decimal Quantity,
        string? BatchNumber
    ) : IRequest<Result<Guid>>;
}
