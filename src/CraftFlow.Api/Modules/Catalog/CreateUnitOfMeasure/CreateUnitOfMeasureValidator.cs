using FluentValidation;

namespace CraftFlow.Api.Modules.Catalog.CreateUnitOfMeasure;

public class CreateUnitOfMeasureValidator : AbstractValidator<CreateUnitOfMeasureCommand>
{
    public CreateUnitOfMeasureValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(20);
    }
}