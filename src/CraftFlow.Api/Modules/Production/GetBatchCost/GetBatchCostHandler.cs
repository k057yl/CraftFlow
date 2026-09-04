using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Production.Domain;
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

        if (recipe == null || recipe.TargetOutputQuantity <= 0)
        {
            return Result.Failure<BatchCostDto>(Error.NotFound(ErrorCodes.General.NOT_FOUND));
        }

        var consumedIngredients = await _dbContext.Set<ConsumedIngredient>()
            .AsNoTracking()
            .Where(ci => ci.ProductionBatchId == batch.Id)
            .ToListAsync(cancellationToken);

        decimal totalRawMaterialCost = 0m;

        foreach (var consumed in consumedIngredients)
        {
            var lot = await _dbContext.StockLots
                .AsNoTracking()
                .FirstOrDefaultAsync(sl => sl.Id == consumed.StockLotId, cancellationToken);

            if (lot != null)
            {
                totalRawMaterialCost += consumed.Quantity * lot.UnitPrice;
            }
        }

        var totalCostWithOverhead = batch.CalculateTotalCost(totalRawMaterialCost);

        var outputQuantity = batch.ActualOutputQuantity > 0
            ? batch.ActualOutputQuantity
            : batch.PlannedOutputQuantity;

        var unitCost = outputQuantity > 0
            ? totalCostWithOverhead / outputQuantity
            : 0m;

        var recipeName = recipe.Name ?? FormattingConstants.NOT_AVAILABLE;

        var dto = new BatchCostDto(
            batch.Id,
            recipeName,
            batch.PlannedOutputQuantity,
            batch.ActualOutputQuantity,
            totalRawMaterialCost,
            totalCostWithOverhead,
            batch.OverheadPercentage,
            unitCost
        );

        return Result.Success(dto);
    }
}