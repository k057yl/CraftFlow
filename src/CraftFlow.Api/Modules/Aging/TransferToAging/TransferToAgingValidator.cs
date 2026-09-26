using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Aging.TransferToAging;
public class TransferToAgingValidator : AbstractValidator<TransferToAgingCommand>
{
    public TransferToAgingValidator(AppDbContext dbContext)
    {
        RuleFor(x => x.ProductionBatchId)
            .MustAsync(async (batchId, ct) => await dbContext.ProductionBatches.AnyAsync(b => b.Id == batchId, ct))
            .WithErrorCode(ErrorCodes.Production.BATCH_NOT_FOUND);

        RuleFor(x => x.AgingChamberId)
            .MustAsync(async (chamberId, ct) => await dbContext.AgingChambers.AnyAsync(c => c.Id == chamberId, ct))
            .WithErrorCode(ErrorCodes.Aging.CHAMBER_NOT_FOUND);
    }
}