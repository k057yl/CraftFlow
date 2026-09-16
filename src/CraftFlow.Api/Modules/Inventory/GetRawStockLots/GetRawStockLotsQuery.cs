using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Inventory.GetRawStockLots;
public record GetRawStockLotsQuery : IRequest<Result<List<RawStockLotLookupDto>>>;