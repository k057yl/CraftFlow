using CraftFlow.Api.Core.Persistence;
using CraftFlow.Api.Modules.ProductionVerticalModules.Fermentation.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.Fermentation.CreateFermentationBatch;

public sealed class CreateFermentationBatchHandler : IRequestHandler<CreateFermentationBatchCommand, Result<Guid>>
{
    private readonly AppDbContext _dbContext;

    public CreateFermentationBatchHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(CreateFermentationBatchCommand request, CancellationToken cancellationToken)
    {
        var prodBatch = await _dbContext.ProductionBatches
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == request.ProductionBatchId, cancellationToken);

        if (prodBatch is null)
            return Result.Failure<Guid>(new Error(ErrorCodes.Fermentation.BATCH_NOT_FOUND));

        var recipeName = await _dbContext.Recipes
            .AsNoTracking()
            .Where(r => r.Id == prodBatch.RecipeId)
            .Select(r => r.Name)
            .FirstOrDefaultAsync(cancellationToken) ?? prodBatch.Name;

        var batch = FermentationBatch.Create(
            prodBatch.TenantId,
            prodBatch.Id,
            prodBatch.Name,
            recipeName,
            request.SubstrateWeightKg,
            request.SaltPercentage,
            request.TargetTemperature,
            request.VesselInfo,
            request.InitialPh
        );

        await _dbContext.FermentationBatches.AddAsync(batch, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(batch.Id);
    }
}