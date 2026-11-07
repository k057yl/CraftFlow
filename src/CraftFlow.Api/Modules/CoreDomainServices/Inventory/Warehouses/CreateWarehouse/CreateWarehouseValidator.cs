using FluentValidation;

namespace CraftFlow.Api.Modules.CoreDomainServices.Inventory.Warehouses.CreateWarehouse
{
    public class CreateWarehouseValidator : AbstractValidator<CreateWarehouseCommand>
    {
        public CreateWarehouseValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(200);
        }
    }
}
