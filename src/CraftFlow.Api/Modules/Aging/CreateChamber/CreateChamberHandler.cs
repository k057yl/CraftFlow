using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Aging.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

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

        var nameExists = await _dbContext.AgingChambers
            .AnyAsync(c => c.Name == request.Name && c.TenantId == tenantId, cancellationToken);

        if (nameExists)
        {
            return Result.Failure<Guid>(Error.Conflict(ErrorCodes.General.ALREADY_EXISTS));
        }

        var chamber = AgingChamber.Create(
            tenantId,
            request.Name,
            request.TargetTemperature,
            request.TargetHumidity
        );

        _dbContext.AgingChambers.Add(chamber);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(chamber.Id);
    }
}