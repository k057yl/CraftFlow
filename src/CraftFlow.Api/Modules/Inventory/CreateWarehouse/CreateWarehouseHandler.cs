using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Inventory.Domain;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Inventory.CreateWarehouse
{
    public class CreateWarehouseHandler : IRequestHandler<CreateWarehouseCommand, Result<Guid>>
    {
        private readonly AppDbContext _dbContext;

        public CreateWarehouseHandler(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<Guid>> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
        {
            var warehouse = Warehouse.Create(request.Name, request.Address);

            _dbContext.Warehouses.Add(warehouse);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(warehouse.Id);
        }
    }
}
