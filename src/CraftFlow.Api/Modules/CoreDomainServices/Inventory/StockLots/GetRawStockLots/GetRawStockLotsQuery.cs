using CraftFlow.SharedKernel.Dtos.Catalog.Inventory;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Inventory.StockLots.GetRawStockLots;
public record GetRawStockLotsQuery : IRequest<Result<List<RawStockLotLookupDto>>>;