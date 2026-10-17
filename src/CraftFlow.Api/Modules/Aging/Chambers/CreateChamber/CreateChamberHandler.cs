using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Aging.Domain;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace CraftFlow.Api.Modules.Aging.Chambers.CreateChamber;

public class CreateChamberHandler : IRequestHandler<CreateChamberCommand, Result<Guid>>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly IMemoryCache _cache;

    public CreateChamberHandler(
        AppDbContext dbContext,
        ITenantContext tenantContext,
        IMemoryCache cache)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _cache = cache;
    }

    public async Task<Result<Guid>> Handle(CreateChamberCommand request, CancellationToken cancellationToken)
    {
        var chamber = AgingChamber.Create(
            _tenantContext.TenantId,
            request.Name,
            request.TargetTemperature,
            request.TargetHumidity
        );

        _dbContext.AgingChambers.Add(chamber);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var cacheKey = $"quota:count:{nameof(AgingChamber)}:{_tenantContext.TenantId}";
        _cache.Remove(cacheKey);

        return Result.Success(chamber.Id);
    }
}