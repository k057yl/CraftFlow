using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Production.CompleteProductionBatch;

public class CompleteProductionBatchValidator : AbstractValidator<CompleteProductionBatchCommand>
{
    public CompleteProductionBatchValidator(AppDbContext dbContext)
    {
        RuleFor(x => x.BatchId)
            .NotEmpty()
            .MustAsync(async (batchId, ct) =>
                await dbContext.ProductionBatches.AnyAsync(b => b.Id == batchId, ct))
            .WithErrorCode(ErrorCodes.Production.BATCH_NOT_FOUND);

        RuleFor(x => x.ActualOutputQuantity)
            .GreaterThan(0);

        RuleFor(x => x.AgingChamberId)
            .MustAsync(async (chamberId, ct) =>
            {
                if (!chamberId.HasValue) return true;
                return await dbContext.AgingChambers.AnyAsync(c => c.Id == chamberId.Value, ct);
            })
            .WithErrorCode(ErrorCodes.Aging.CHAMBER_NOT_FOUND);
    }
}