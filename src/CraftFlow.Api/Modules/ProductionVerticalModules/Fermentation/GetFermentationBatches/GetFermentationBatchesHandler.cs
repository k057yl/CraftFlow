using CraftFlow.Api.Core.Persistence;
using CraftFlow.SharedKernel.Dtos.Fermentation;
using CraftFlow.SharedKernel.Enums.Fermrntation;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.Fermentation.GetFermentationBatches;

public sealed class GetFermentationBatchesHandler : IRequestHandler<GetFermentationBatchesQuery, Result<List<FermentationBatchDto>>>
{
    private readonly AppDbContext _dbContext;

    public GetFermentationBatchesHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<FermentationBatchDto>>> Handle(GetFermentationBatchesQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.FermentationBatches
            .AsNoTracking()
            .Include(b => b.Readings)
            .AsQueryable();

        if (!request.IncludeCompleted)
        {
            query = query.Where(b => b.Stage == FermentationStage.ACTIVE_WARM || b.Stage == FermentationStage.COLD_RETARDATION);
        }

        var now = DateTime.UtcNow;
        var batches = await query
            .OrderByDescending(b => b.StartedAt)
            .ToListAsync(cancellationToken);

        var result = batches.Select(b => new FermentationBatchDto(
            b.Id,
            b.ProductionBatchId,
            b.BatchNumber,
            b.RecipeName,
            b.SubstrateWeightKg,
            b.SaltPercentage,
            b.CalculatedSaltGrams,
            b.InitialPh,
            b.CurrentPh,
            b.TargetTemperature,
            b.VesselInfo,
            b.Stage,
            b.StartedAt,
            b.MovedToColdAt,
            b.CompletedAt,
            (int)Math.Max(0, (now - b.StartedAt).TotalDays),
            b.Readings
                .OrderByDescending(r => r.RecordedAt)
                .Select(r => new FermentationReadingDto(
                    r.Id,
                    r.RecordedAt,
                    r.Ph,
                    r.Temperature,
                    r.DegassedOrBurped,
                    r.BrineToppedUp,
                    r.SensoryNotes))
                .ToList()
        )).ToList();

        return Result.Success(result);
    }
}