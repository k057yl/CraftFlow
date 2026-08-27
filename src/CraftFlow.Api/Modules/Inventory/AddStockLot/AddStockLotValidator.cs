using FluentValidation;

namespace CraftFlow.Api.Modules.Inventory.AddStockLot
{
    public class AddStockLotValidator : AbstractValidator<AddStockLotCommand>
    {
        public AddStockLotValidator()
        {
            RuleFor(x => x.WarehouseId).NotEmpty();
            RuleFor(x => x.ItemId).NotEmpty();
            RuleFor(x => x.Quantity).GreaterThan(0);
        }
    }
}
