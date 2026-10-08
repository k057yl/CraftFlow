using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Procurement.Suppliers;

public class CreateSupplierValidator : AbstractValidator<CreateSupplierCommand>
{
    public CreateSupplierValidator(AppDbContext dbContext, ITenantContext tenantContext)
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Procurement.SUPPLIER_NAME_REQUIRED)
            .MustAsync(async (name, ct) =>
            {
                if (string.IsNullOrWhiteSpace(name)) return true;
                var sanitized = name.Trim();
                return !await dbContext.Suppliers.AnyAsync(s => s.Name == sanitized && s.TenantId == tenantContext.TenantId, ct);
            })
            .WithErrorCode(ErrorCodes.General.ALREADY_EXISTS);

        RuleFor(x => x.Email)
            .EmailAddress()
            .WithErrorCode(ErrorCodes.General.INVALID_EMAIL_FORMAT)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Phone)
            .Cascade(CascadeMode.Stop)
            .Matches(@"^\+?[0-9\s\-\(\)]+$")
            .WithErrorCode(ErrorCodes.Supplier.PHONE_NUMBER_INVALID_FORMAT)
            .Must(phone =>
            {
                if (string.IsNullOrWhiteSpace(phone)) return true;
                var digitsCount = phone.Count(char.IsDigit);
                return digitsCount is >= 7 and <= 15;
            })
            .WithErrorCode(ErrorCodes.Supplier.PHONE_NUMBER_INVALID_FORMAT)
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));
    }
}