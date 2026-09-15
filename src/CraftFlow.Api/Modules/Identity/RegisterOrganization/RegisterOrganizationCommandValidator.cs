using CraftFlow.SharedKernel.Constants;
using FluentValidation;

namespace CraftFlow.Api.Modules.Identity.RegisterOrganization;

public sealed class RegisterOrganizationCommandValidator : AbstractValidator<RegisterOrganizationCommand>
{
    public RegisterOrganizationCommandValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.General.VALUE_REQUIRED)
            .MaximumLength(200);

        RuleFor(x => x.OwnerFullName)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.General.VALUE_REQUIRED)
            .MaximumLength(150);

        RuleFor(x => x.OwnerEmail)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.General.VALUE_REQUIRED)
            .EmailAddress()
            .WithErrorCode(ErrorCodes.Auth.INVALID_CREDENTIALS);

        RuleFor(x => x.OwnerPassword)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.General.VALUE_REQUIRED)
            .MinimumLength(6)
            .WithErrorCode(ErrorCodes.Auth.INVALID_CREDENTIALS);
    }
}