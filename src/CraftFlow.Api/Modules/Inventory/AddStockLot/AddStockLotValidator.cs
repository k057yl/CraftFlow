using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Inventory.AddStockLot;

public class AddStockLotValidator : AbstractValidator<AddStockLotCommand>
{
    public AddStockLotValidator(AppDbContext dbContext)
    {
        RuleFor(x => x.WarehouseId)
            .NotEmpty()
            .MustAsync(async (warehouseId, ct) =>
                await dbContext.Warehouses.AnyAsync(w => w.Id == warehouseId, ct))
            .WithErrorCode(ErrorCodes.Inventory.WAREHOUSE_NOT_FOUND);

        RuleFor(x => x.ItemId)
            .NotEmpty();

        RuleFor(x => x.Quantity)
            .GreaterThan(0);

        RuleFor(x => x.UnitsCount)
            .GreaterThan(0)
            .When(x => x.UnitsCount.HasValue);

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.ExpirationDate)
            .Must(date => date == null || date.Value > DateTime.UtcNow)
            .WithErrorCode(ErrorCodes.Production.EXPIRATION_DATE_MUST_BE_IN_FUTURE);
    }
}