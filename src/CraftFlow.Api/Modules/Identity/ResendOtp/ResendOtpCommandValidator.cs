using CraftFlow.SharedKernel.Constants;
using FluentValidation;

namespace CraftFlow.Api.Modules.Identity.ResendOtp;

public sealed class ResendOtpCommandValidator : AbstractValidator<ResendOtpCommand>
{
    public ResendOtpCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.General.VALUE_REQUIRED)
            .EmailAddress()
            .WithErrorCode(ErrorCodes.Auth.INVALID_CREDENTIALS);
    }
}