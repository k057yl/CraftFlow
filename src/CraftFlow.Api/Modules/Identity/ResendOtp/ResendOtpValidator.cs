using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Identity.ResendOtp;

public class ResendOtpValidator : AbstractValidator<ResendOtp>
{
    public ResendOtpValidator(AppDbContext dbContext)
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MustAsync(async (email, ct) =>
            {
                var normalizedEmail = email.Trim().ToLowerInvariant();
                return await dbContext.Users
                    .IgnoreQueryFilters()
                    .AnyAsync(u => u.Email == normalizedEmail, ct);
            })
            .WithErrorCode(ErrorCodes.Auth.USER_NOT_FOUND);
    }
}