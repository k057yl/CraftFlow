using CraftFlow.Api.Core.Persistence;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.Fermentation.AddFermentationReading;
public sealed class AddFermentationReadingHandler : IRequestHandler<AddFermentationReadingCommand, Result<Guid>>
{
    private readonly AppDbContext _dbContext;

    public AddFermentationReadingHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(AddFermentationReadingCommand request, CancellationToken cancellationToken)
    {
        var batch = await _dbContext.FermentationBatches
            .Include(b => b.Readings)
            .FirstOrDefaultAsync(b => b.Id == request.FermentationBatchId, cancellationToken);

        if (batch is null)
            return Result.Failure<Guid>(new Error(ErrorCodes.Fermentation.BATCH_NOT_FOUND));

        var reading = batch.AddReading(
            request.Ph,
            request.Temperature,
            request.DegassedOrBurped,
            request.BrineToppedUp,
            request.SensoryNotes
        );

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(reading.Id);
    }
}