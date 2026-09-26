using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Identity.RegisterOrganization;

public class RegisterOrganizationValidator : AbstractValidator<RegisterOrganizationCommand>
{
    public RegisterOrganizationValidator(AppDbContext dbContext)
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.OwnerFullName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.OwnerEmail)
            .NotEmpty()
            .EmailAddress()
            .MustAsync(async (email, ct) =>
            {
                var normalizedEmail = email.Trim().ToLowerInvariant();
                var exists = await dbContext.Users
                    .IgnoreQueryFilters()
                    .AnyAsync(u => u.Email == normalizedEmail, ct);

                return !exists;
            })
            .WithErrorCode(ErrorCodes.Auth.USER_ALREADY_EXISTS);

        RuleFor(x => x.OwnerPassword)
            .NotEmpty()
            .MinimumLength(6);
    }
}