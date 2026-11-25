using CraftFlow.Api.Core.MultiTenancy;
using CraftFlow.Api.Core.Persistence;
using CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Planning.PlanProductionBatch;

public sealed class PlanCheeseBatchCommandHandler : IRequestHandler<PlanCheeseBatchCommand, Result<Guid>>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public PlanCheeseBatchCommandHandler(AppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(PlanCheeseBatchCommand request, CancellationToken cancellationToken)
    {
        if (request.RecipeId == Guid.Empty || request.WarehouseId == Guid.Empty || request.PlannedOutputQuantity <= 0)
        {
            return Result.Failure<Guid>(Error.Validation(ErrorCodes.General.VALUE_REQUIRED));
        }

        var recipe = await _dbContext.Recipes
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.RecipeId, cancellationToken);

        if (recipe == null)
        {
            return Result.Failure<Guid>(Error.NotFound(ErrorCodes.Catalog.RECIPE_NOT_FOUND));
        }

        var batch = CheeseBatch.Create(
            request.RecipeId,
            recipe.ProductId,
            request.WarehouseId,
            request.DestinationWarehouseId,
            equipmentLocationId: Guid.Empty,
            request.PlannedOutputQuantity,
            recipe.TargetDurationMinutes,
            request.Name,
            request.ScheduledAt
        );

        _dbContext.CheeseBatches.Add(batch);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(batch.Id);
    }
}