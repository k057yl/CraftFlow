using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Inventory.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace CraftFlow.Api.Modules.Inventory.Warehouses.CreateWarehouse;

public class CreateWarehouseHandler : IRequestHandler<CreateWarehouseCommand, Result<Guid>>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly IMemoryCache _cache;

    public CreateWarehouseHandler(
        AppDbContext dbContext,
        ITenantContext tenantContext,
        IMemoryCache cache)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _cache = cache;
    }

    public async Task<Result<Guid>> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.IsResolved || _tenantContext.TenantId == Guid.Empty)
        {
            return Result.Failure<Guid>(ErrorCodes.Auth.UNAUTHORIZED);
        }

        var warehouse = Warehouse.Create(
            _tenantContext.TenantId,
            request.Name,
            request.Type,
            request.Address
        );

        _dbContext.Warehouses.Add(warehouse);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var cacheKey = $"quota:count:{nameof(Warehouse)}:{_tenantContext.TenantId}";
        _cache.Remove(cacheKey);

        return Result.Success(warehouse.Id);
    }
}