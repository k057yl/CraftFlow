using CraftFlow.SharedKernel.Constants;
using FluentValidation;

namespace CraftFlow.Api.Modules.CoreDomainServices.Identity.Auth.LoginUser;

public sealed class LoginUserValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.General.VALUE_REQUIRED)
            .EmailAddress()
            .WithErrorCode(ErrorCodes.Auth.INVALID_CREDENTIALS);

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.General.VALUE_REQUIRED);
    }
}