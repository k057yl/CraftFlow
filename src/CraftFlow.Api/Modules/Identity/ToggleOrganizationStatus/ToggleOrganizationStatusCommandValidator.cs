using CraftFlow.SharedKernel.Constants;
using FluentValidation;

namespace CraftFlow.Api.Modules.Identity.ToggleOrganizationStatus;

public sealed class ToggleOrganizationStatusCommandValidator : AbstractValidator<ToggleOrganizationStatusCommand>
{
    public ToggleOrganizationStatusCommandValidator()
    {
        RuleFor(x => x.OrganizationId)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.General.VALUE_REQUIRED);
    }
}