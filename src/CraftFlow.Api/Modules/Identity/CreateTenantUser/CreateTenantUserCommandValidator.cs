using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Identity.CreateTenantUser;

public class CreateTenantUserValidator : AbstractValidator<CreateTenantUserCommand>
{
    public CreateTenantUserValidator(AppDbContext dbContext)
    {
        RuleFor(x => x.Email)
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

        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6);
    }
}