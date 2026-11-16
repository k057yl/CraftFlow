using CraftFlow.Api.Core.Persistence;
using CraftFlow.Api.Modules.CoreDomainServices.Production.Batches.StartProductionBatch;
using CraftFlow.Api.Modules.ProductionVerticalModules.Fermentation.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.Fermentation.CreateFermentationBatch;

public sealed class CreateFermentationBatchHandler : IRequestHandler<CreateFermentationBatchCommand, Result<Guid>>
{
    private readonly AppDbContext _dbContext;
    private readonly ISender _sender;

    public CreateFermentationBatchHandler(AppDbContext dbContext, ISender sender)
    {
        _dbContext = dbContext;
        _sender = sender;
    }

    public async Task<Result<Guid>> Handle(CreateFermentationBatchCommand request, CancellationToken cancellationToken)
    {
        var startProdResult = await _sender.Send(new StartProductionBatchCommand(
            RecipeId: request.RecipeId,
            WarehouseId: request.WarehouseId,
            DestinationWarehouseId: request.DestinationWarehouseId,
            PlannedOutputQuantity: request.SubstrateWeightKg,
            EquipmentLocationId: request.EquipmentLocationId,
            Name: request.BatchName
        ), cancellationToken);

        if (startProdResult.IsFailure)
            return Result.Failure<Guid>(startProdResult.Error);

        var prodBatchId = startProdResult.Value;

        var prodBatch = await _dbContext.ProductionBatches
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == prodBatchId, cancellationToken);

        if (prodBatch is null)
            return Result.Failure<Guid>(new Error(ErrorCodes.Fermentation.BATCH_NOT_FOUND));

        var recipeName = await _dbContext.Recipes
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(r => r.Id == request.RecipeId)
            .Select(r => r.Name)
            .FirstOrDefaultAsync(cancellationToken) ?? prodBatch.Name;

        var vesselName = await _dbContext.StorageLocations
            .AsNoTracking()
            .Where(l => l.Id == request.EquipmentLocationId)
            .Select(l => l.Name)
            .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

        var batch = FermentationBatch.Create(
            prodBatch.TenantId,
            prodBatch.Id,
            prodBatch.Name,
            recipeName,
            request.SubstrateWeightKg,
            request.SaltPercentage,
            request.TargetTemperature,
            vesselName,
            request.InitialPh
        );

        await _dbContext.FermentationBatches.AddAsync(batch, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(batch.Id);
    }
}