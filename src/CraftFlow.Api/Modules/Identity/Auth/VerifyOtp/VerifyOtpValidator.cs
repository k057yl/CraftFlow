using CraftFlow.SharedKernel.Constants;
using FluentValidation;

namespace CraftFlow.Api.Modules.Identity.Auth.VerifyOtp;

public sealed class VerifyOtpValidator : AbstractValidator<VerifyOtpCommand>
{
    public VerifyOtpValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.General.VALUE_REQUIRED)
            .EmailAddress()
            .WithErrorCode(ErrorCodes.Auth.INVALID_CREDENTIALS);

        RuleFor(x => x.OtpCode)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.General.VALUE_REQUIRED)
            .Length(6)
            .WithErrorCode(ErrorCodes.Auth.INVALID_CREDENTIALS)
            .Matches(@"^\d{6}$")
            .WithErrorCode(ErrorCodes.Auth.INVALID_CREDENTIALS);
    }
}