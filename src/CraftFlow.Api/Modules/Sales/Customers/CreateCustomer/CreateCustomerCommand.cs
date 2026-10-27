using CraftFlow.Api.Common.Authorization;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Sales.Customers.CreateCustomer;

public record CreateCustomerCommand(string Name, string? Phone) : IRequest<Result<Guid>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.CUSTOMERS_MANAGE;
}