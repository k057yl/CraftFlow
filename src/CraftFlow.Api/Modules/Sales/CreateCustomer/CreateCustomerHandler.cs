using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Sales.Domain;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Sales.CreateCustomer
{
    public class CreateCustomerHandler : IRequestHandler<CreateCustomerCommand, Result<Guid>>
    {
        private readonly AppDbContext _dbContext;

        public CreateCustomerHandler(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<Guid>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = Customer.Create(request.Name, request.Phone);

            _dbContext.Customers.Add(customer);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(customer.Id);
        }
    }
}
