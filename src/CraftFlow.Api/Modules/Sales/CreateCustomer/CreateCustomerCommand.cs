using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Sales.CreateCustomer
{
    public record CreateCustomerCommand(string Name, string? Phone) : IRequest<Result<Guid>>;
}
