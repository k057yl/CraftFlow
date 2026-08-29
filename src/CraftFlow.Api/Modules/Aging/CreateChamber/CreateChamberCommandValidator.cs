using CraftFlow.SharedKernel.Constants;
using FluentValidation;

namespace CraftFlow.Api.Modules.Aging.CreateChamber;
public class CreateChamberCommandValidator : AbstractValidator<CreateChamberCommand>
{
    public CreateChamberCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Aging.CHAMBER_NAME_REQUIRED);

        RuleFor(x => x.TargetTemperature)
            .InclusiveBetween(-10m, 35m)
            .WithErrorCode(ErrorCodes.General.INVALID_FORMAT);

        RuleFor(x => x.TargetHumidity)
            .InclusiveBetween(0m, 100m)
            .WithErrorCode(ErrorCodes.General.INVALID_FORMAT);
    }
}
