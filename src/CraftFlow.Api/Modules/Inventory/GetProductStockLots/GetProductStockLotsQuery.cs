using CraftFlow.SharedKernel.Dtos.Inventory;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Inventory.GetProductStockLots;
public record GetProductStockLotsQuery : IRequest<Result<List<ProductStockLotLookupDto>>>;