using CraftFlow.Api.Common.Authorization;
using CraftFlow.SharedKernel.Dtos.Sale;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Sales.Stock.GetStockLotDetails;

public sealed record GetStockLotDetailsQuery(Guid StockLotId) : IRequest<Result<StockLotDetailsDto>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.SALES_VIEW;
}