using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Sales.GetCustomers
{
    public class GetCustomersHandler : IRequestHandler<GetCustomersQuery, Result<List<CustomerDto>>>
    {
        private readonly AppDbContext _dbContext;

        public GetCustomersHandler(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<List<CustomerDto>>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
        {
            var customers = await _dbContext.Customers
                .AsNoTracking()
                .Select(c => new CustomerDto(c.Id, c.Name))
                .ToListAsync(cancellationToken);

            return Result.Success(customers);
        }
    }
}
