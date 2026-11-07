using CraftFlow.Api.Core.Authorization;
using CraftFlow.SharedKernel.Dtos.Sale;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Sales.Customers.GetCustomers;

public record GetCustomersQuery : IRequest<Result<List<CustomerDto>>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.SALES_VIEW;
}