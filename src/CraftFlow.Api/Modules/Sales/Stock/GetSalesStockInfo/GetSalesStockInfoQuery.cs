using CraftFlow.SharedKernel.Dtos.Sale;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Sales.Stock.GetSalesStockInfo;
public record GetSalesStockInfoQuery(
    Guid WarehouseId,
    Guid ProductId
) : IRequest<Result<SalesStockInfoDto>>;