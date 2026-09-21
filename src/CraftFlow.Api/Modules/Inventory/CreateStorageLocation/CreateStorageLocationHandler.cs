using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Inventory.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
        var tenantId = _tenantContext.TenantId;

        if (tenantId == Guid.Empty)
        {
            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == _tenantContext.UserId, cancellationToken);

            if (user != null && user.TenantId != Guid.Empty)
            {
                tenantId = user.TenantId;
            }
        }

        if (tenantId == Guid.Empty)
        {
            return Result.Failure<Guid>(Error.Validation(ErrorCodes.Auth.ACCESS_DENIED));
        }

        if (request.WarehouseId.HasValue)
        {
            var warehouseExists = await _dbContext.Warehouses
                .AnyAsync(w => w.Id == request.WarehouseId.Value, cancellationToken);

            if (!warehouseExists)
            {
                return Result.Failure<Guid>(Error.NotFound(ErrorCodes.Inventory.WAREHOUSE_NOT_FOUND));
            }
        }

        if (request.ChamberId.HasValue)
        {
            var chamberExists = await _dbContext.AgingChambers
                .AnyAsync(c => c.Id == request.ChamberId.Value, cancellationToken);

            if (!chamberExists)
            {
                return Result.Failure<Guid>(Error.NotFound(ErrorCodes.Aging.CHAMBER_NOT_FOUND));
            }
        }

        var storageLocation = StorageLocation.Create(
            tenantId,
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