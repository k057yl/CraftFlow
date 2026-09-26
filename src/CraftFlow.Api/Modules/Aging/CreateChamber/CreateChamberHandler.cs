using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Aging.Domain;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Aging.CreateChamber;

public class CreateChamberHandler : IRequestHandler<CreateChamberCommand, Result<Guid>>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public CreateChamberHandler(AppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
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

        return Result.Success(chamber.Id);
    }
}