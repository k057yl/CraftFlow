using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Production.Domain;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Production.GetBatchesReadyForAging;

public class GetBatchesReadyForAgingHandler : IRequestHandler<GetBatchesReadyForAgingQuery, Result<List<BatchReadyForAgingDto>>>
{
    private readonly AppDbContext _dbContext;

    public GetBatchesReadyForAgingHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<BatchReadyForAgingDto>>> Handle(GetBatchesReadyForAgingQuery request, CancellationToken cancellationToken)
    {
        var existingAgingBatchIds = await _dbContext.AgingLots
            .AsNoTracking()
            .Select(l => l.ProductionBatchId)
            .ToListAsync(cancellationToken);

        var batches = await _dbContext.ProductionBatches
            .AsNoTracking()
            .Where(b => (b.State == BatchState.Completed || b.State == BatchState.ReadyForAging)
                     && !existingAgingBatchIds.Contains(b.Id))
            .Join(_dbContext.Recipes,
                  batch => batch.RecipeId,
                  recipe => recipe.Id,
                  (batch, recipe) => new { Batch = batch, Recipe = recipe })
            .Where(br => br.Recipe.IsAgingRequired)
            .Select(br => new BatchReadyForAgingDto(
                br.Batch.Id,
                string.IsNullOrWhiteSpace(br.Batch.Name)
                    ? $"Party #{br.Batch.Id.ToString().Substring(0, 8)} (Exit: {(br.Batch.ActualOutputQuantity > 0 ? br.Batch.ActualOutputQuantity : br.Batch.PlannedOutputQuantity)} кг)"
                    : $"{br.Batch.Name} (Exit: {(br.Batch.ActualOutputQuantity > 0 ? br.Batch.ActualOutputQuantity : br.Batch.PlannedOutputQuantity)} kg)",
                br.Recipe.DefaultMinAgingDays ?? 0
            ))
            .ToListAsync(cancellationToken);

        return Result.Success(batches);
    }
}