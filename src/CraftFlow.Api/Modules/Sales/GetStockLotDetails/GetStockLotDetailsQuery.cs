using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Sales.GetStockLotDetails;

public sealed record GetStockLotDetailsQuery(Guid StockLotId) : IRequest<Result<StockLotDetailsDto>>;