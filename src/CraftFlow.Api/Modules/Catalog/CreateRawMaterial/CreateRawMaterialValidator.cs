using FluentValidation;

namespace CraftFlow.Api.Modules.Catalog.CreateRawMaterial
{
    public class CreateRawMaterialValidator : AbstractValidator<CreateRawMaterialCommand>
    {
        public CreateRawMaterialValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.UnitOfMeasureId)
                .NotEmpty();
        }
    }
}
