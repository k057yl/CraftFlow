using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Sales.GetCustomers
{
    public record GetCustomersQuery : IRequest<Result<List<CustomerDto>>>;
}
