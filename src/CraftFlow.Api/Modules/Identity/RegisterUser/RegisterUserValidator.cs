using FluentValidation;

namespace CraftFlow.Api.Modules.Identity.RegisterUser;

public class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches(@"[A-Z]").WithMessage("PASSWORD_NEED_UPPER")
            .Matches(@"[a-z]").WithMessage("PASSWORD_NEED_LOWER")
            .Matches(@"[0-9]").WithMessage("PASSWORD_NEED_DIGIT")
            .Matches(@"[\!\?\*\.]").WithMessage("PASSWORD_NEED_SPECIAL");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password)
            .WithMessage("PASSWORDS_DO_NOT_MATCH");

        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(200);
    }
}