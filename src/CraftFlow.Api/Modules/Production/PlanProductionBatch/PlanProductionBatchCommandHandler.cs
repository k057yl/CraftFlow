using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Production.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Production.PlanProductionBatch;

public sealed class PlanProductionBatchCommandHandler : IRequestHandler<PlanProductionBatchCommand, Result<Guid>>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public PlanProductionBatchCommandHandler(AppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(PlanProductionBatchCommand request, CancellationToken cancellationToken)
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

        var batch = ProductionBatch.Create(
            request.RecipeId,
            recipe.ProductId,
            request.WarehouseId,
            request.DestinationWarehouseId,
            request.PlannedOutputQuantity,
            recipe.TargetDurationMinutes,
            request.Name
        );

        _dbContext.ProductionBatches.Add(batch);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(batch.Id);
    }
}