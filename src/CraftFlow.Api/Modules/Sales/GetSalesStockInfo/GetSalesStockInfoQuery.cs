using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Sales.GetSalesStockInfo;
public record GetSalesStockInfoQuery(
    Guid WarehouseId,
    Guid ProductId
) : IRequest<Result<SalesStockInfoDto>>;