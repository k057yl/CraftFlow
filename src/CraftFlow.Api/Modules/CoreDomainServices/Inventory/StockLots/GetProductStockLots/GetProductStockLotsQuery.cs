using CraftFlow.SharedKernel.Dtos.Catalog.Inventory;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Inventory.StockLots.GetProductStockLots;
public record GetProductStockLotsQuery : IRequest<Result<List<ProductStockLotLookupDto>>>;