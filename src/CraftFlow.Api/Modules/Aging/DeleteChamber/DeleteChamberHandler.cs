using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Aging.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Aging.DeleteChamber;

public class DeleteChamberHandler : IRequestHandler<DeleteChamberCommand, Result<bool>>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public DeleteChamberHandler(AppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<Result<bool>> Handle(DeleteChamberCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        var chamber = await _dbContext.AgingChambers
            .FirstOrDefaultAsync(c => c.Id == request.Id && c.TenantId == tenantId, cancellationToken);

        if (chamber == null)
        {
            return Result.Failure<bool>(Error.NotFound(ErrorCodes.General.NOT_FOUND));
        }

        var hasActiveLots = await _dbContext.AgingLots
            .AnyAsync(l => l.AgingChamberId == request.Id && l.State == AgingState.InChamber, cancellationToken);

        if (hasActiveLots)
        {
            return Result.Failure<bool>(Error.Validation("CHAMBER_HAS_ACTIVE_LOTS"));
        }

        _dbContext.AgingChambers.Remove(chamber);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}