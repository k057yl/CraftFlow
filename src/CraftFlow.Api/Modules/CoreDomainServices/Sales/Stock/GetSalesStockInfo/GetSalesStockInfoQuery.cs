using CraftFlow.Api.Core.Authorization;
using CraftFlow.SharedKernel.Dtos.Sale;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Sales.Stock.GetSalesStockInfo;

public record GetSalesStockInfoQuery(
    Guid WarehouseId,
    Guid ProductId
) : IRequest<Result<SalesStockInfoDto>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.SALES_VIEW;
}