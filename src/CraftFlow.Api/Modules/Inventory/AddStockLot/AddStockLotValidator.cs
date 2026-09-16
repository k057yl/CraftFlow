using CraftFlow.SharedKernel.Constants;
using FluentValidation;

namespace CraftFlow.Api.Modules.Inventory.AddStockLot;

public class AddStockLotValidator : AbstractValidator<AddStockLotCommand>
{
    public AddStockLotValidator()
    {
        RuleFor(x => x.WarehouseId)
            .NotEmpty();

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
            .WithMessage(ErrorCodes.Production.EXPIRATION_DATE_MUST_BE_IN_FUTURE);
    }
}