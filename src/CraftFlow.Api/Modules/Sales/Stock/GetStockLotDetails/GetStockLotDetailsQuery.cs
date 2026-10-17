using CraftFlow.SharedKernel.Dtos.Sale;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Sales.Stock.GetStockLotDetails;

public sealed record GetStockLotDetailsQuery(Guid StockLotId) : IRequest<Result<StockLotDetailsDto>>;