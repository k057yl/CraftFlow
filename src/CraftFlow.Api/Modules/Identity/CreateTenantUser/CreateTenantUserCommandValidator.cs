using CraftFlow.SharedKernel.Constants;
using FluentValidation;

namespace CraftFlow.Api.Modules.Identity.CreateTenantUser;

public sealed class CreateTenantUserCommandValidator : AbstractValidator<CreateTenantUserCommand>
{
    public CreateTenantUserCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.General.VALUE_REQUIRED)
            .MaximumLength(150);

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.General.VALUE_REQUIRED)
            .EmailAddress()
            .WithErrorCode(ErrorCodes.Auth.INVALID_CREDENTIALS);

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.General.VALUE_REQUIRED)
            .MinimumLength(6)
            .WithErrorCode(ErrorCodes.Auth.INVALID_CREDENTIALS);
    }
}