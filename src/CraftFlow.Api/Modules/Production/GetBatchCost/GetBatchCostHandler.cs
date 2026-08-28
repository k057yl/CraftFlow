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
            .Include(r => r.Ingredients)
            .FirstOrDefaultAsync(r => r.Id == batch.RecipeId, cancellationToken);

        if (recipe == null || recipe.TargetOutputQuantity <= 0)
        {
            return Result.Failure<BatchCostDto>(Error.NotFound(ErrorCodes.General.NOT_FOUND));
        }

        var planMultiplier = batch.PlannedOutputQuantity / recipe.TargetOutputQuantity;
        decimal totalRawMaterialCost = 0m;

        foreach (var ingredient in recipe.Ingredients)
        {
            var requiredQty = ingredient.Quantity * planMultiplier;

            var avgUnitPrice = await _dbContext.StockLots
                .Where(s => s.ItemId == ingredient.RawMaterialId)
                .Select(s => (decimal?)s.UnitPrice)
                .AverageAsync(cancellationToken) ?? 0m;

            totalRawMaterialCost += requiredQty * avgUnitPrice;
        }

        var outputQuantity = batch.ActualOutputQuantity > 0
            ? batch.ActualOutputQuantity
            : batch.PlannedOutputQuantity;

        var unitCost = outputQuantity > 0
            ? totalRawMaterialCost / outputQuantity
            : 0m;

        var dto = new BatchCostDto(
            batch.Id,
            recipe.Name ?? "N/A",
            batch.PlannedOutputQuantity,
            batch.ActualOutputQuantity,
            totalRawMaterialCost,
            unitCost
        );

        return Result.Success(dto);
    }
}