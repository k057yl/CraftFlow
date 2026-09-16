using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Inventory.GetStockLots;
public record GetStockLotsQuery(
    Guid? WarehouseId = null,
    Guid? SupplierId = null,
    bool? OnlyExpiringSoon = null,
    bool? OnlyExpired = null,
    string? SortBy = null
) : IRequest<Result<List<StockLotDto>>>;