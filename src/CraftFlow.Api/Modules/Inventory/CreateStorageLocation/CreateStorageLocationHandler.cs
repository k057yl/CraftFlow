using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Inventory.Domain;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Inventory.CreateStorageLocation;

public class CreateStorageLocationHandler : IRequestHandler<CreateStorageLocationCommand, Result<Guid>>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public CreateStorageLocationHandler(AppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(CreateStorageLocationCommand request, CancellationToken cancellationToken)
    {
        var storageLocation = StorageLocation.Create(
            _tenantContext.TenantId,
            request.Name,
            request.LocationType,
            request.WarehouseId,
            request.ChamberId,
            request.Capacity
        );

        _dbContext.StorageLocations.Add(storageLocation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(storageLocation.Id);
    }
}