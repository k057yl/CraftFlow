using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Aging.ReleaseFromAging;

public class ReleaseFromAgingValidator : AbstractValidator<ReleaseFromAgingCommand>
{
    public ReleaseFromAgingValidator(AppDbContext dbContext)
    {
        RuleFor(x => x.AgingLotId)
            .NotEmpty()
            .MustAsync(async (lotId, ct) =>
                await dbContext.AgingLots.AnyAsync(l => l.Id == lotId, ct))
            .WithErrorCode(ErrorCodes.General.NOT_FOUND);

        RuleFor(x => x.TargetWarehouseId)
            .NotEmpty()
            .MustAsync(async (warehouseId, ct) =>
                await dbContext.Warehouses.AnyAsync(w => w.Id == warehouseId, ct))
            .WithErrorCode(ErrorCodes.Inventory.WAREHOUSE_NOT_FOUND);

        RuleFor(x => x.ActualFinalQuantity)
            .GreaterThan(0)
            .WithErrorCode(ErrorCodes.General.VALUE_REQUIRED);

        RuleFor(x => x.UnitsCount)
            .GreaterThan(0)
            .WithErrorCode(ErrorCodes.General.VALUE_REQUIRED);

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0)
            .WithErrorCode(ErrorCodes.General.INVALID_FORMAT);
    }
}