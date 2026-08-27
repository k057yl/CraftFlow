using FluentValidation;

namespace CraftFlow.Api.Modules.Catalog.CreateProduct
{
    public class CreateProductValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.UnitOfMeasureId)
                .NotEmpty();
        }
    }
}