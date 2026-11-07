using CraftFlow.SharedKernel.Dtos.Catalog.Inventory;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Inventory.StockLots.GetStockLots;
public record GetStockLotsQuery(
    Guid? WarehouseId = null,
    Guid? SupplierId = null,
    bool? OnlyExpiringSoon = null,
    bool? OnlyExpired = null,
    string? SortBy = null
) : IRequest<Result<List<StockLotDto>>>;