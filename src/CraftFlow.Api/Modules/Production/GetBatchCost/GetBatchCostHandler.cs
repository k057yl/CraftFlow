using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Production.GetBatchCost;
public class GetBatchCostHandler : IRequestHandler<GetBatchCostQuery, Result<BatchCostDto>>
{
    private readonly AppDbContext _dbContext;

    public GetBatchCostHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<BatchCostDto>> Handle(GetBatchCostQuery request, CancellationToken cancellationToken)
    {
        var batch = await _dbContext.ProductionBatches
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == request.BatchId, cancellationToken);

        if (batch == null)
        {
            return Result.Failure<BatchCostDto>(Error.NotFound(ErrorCodes.Production.BATCH_NOT_FOUND));
        }

        var recipe = await _dbContext.Recipes
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == batch.RecipeId, cancellationToken);

        var totalRawMaterialCost = batch.ActualOutputQuantity * 45.50m;
        var unitCost = batch.ActualOutputQuantity > 0
            ? totalRawMaterialCost / batch.ActualOutputQuantity
            : 0m;

        var dto = new BatchCostDto(
            batch.Id,
            recipe?.Name ?? "N/A",
            batch.PlannedOutputQuantity,
            batch.ActualOutputQuantity,
            totalRawMaterialCost,
            unitCost
        );

        return Result.Success(dto);
    }
}
