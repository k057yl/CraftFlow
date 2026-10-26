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
            .MaximumLength(200)
            .WithErrorCode(ErrorCodes.General.MAX_LENGTH_EXCEEDED)
            .MustAsync(async (name, ct) =>
            {
                if (string.IsNullOrWhiteSpace(name)) return true;
                var sanitized = name.Trim();
                return !await dbContext.Suppliers.AnyAsync(s => s.Name == sanitized && s.TenantId == tenantContext.TenantId, ct);
            })
            .WithErrorCode(ErrorCodes.General.ALREADY_EXISTS);

        RuleFor(x => x.LegalName)
            .MaximumLength(250)
            .WithErrorCode(ErrorCodes.General.MAX_LENGTH_EXCEEDED)
            .When(x => !string.IsNullOrWhiteSpace(x.LegalName));

        RuleFor(x => x.TaxId)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(20)
            .WithErrorCode(ErrorCodes.General.MAX_LENGTH_EXCEEDED)
            .Matches(@"^[0-9]+$")
            .WithErrorCode(ErrorCodes.Supplier.TAX_ID_INVALID_FORMAT)
            .MustAsync(async (taxId, ct) =>
            {
                if (string.IsNullOrWhiteSpace(taxId)) return true;
                var sanitized = taxId.Trim();
                return !await dbContext.Suppliers.AnyAsync(s => s.TaxId == sanitized && s.TenantId == tenantContext.TenantId, ct);
            })
            .WithErrorCode(ErrorCodes.Supplier.TAX_ID_ALREADY_EXISTS)
            .When(x => !string.IsNullOrWhiteSpace(x.TaxId));

        RuleFor(x => x.Email)
            .MaximumLength(100)
            .WithErrorCode(ErrorCodes.General.MAX_LENGTH_EXCEEDED)
            .EmailAddress()
            .WithErrorCode(ErrorCodes.General.INVALID_EMAIL_FORMAT)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Phone)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(50)
            .WithErrorCode(ErrorCodes.General.MAX_LENGTH_EXCEEDED)
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

        RuleFor(x => x.Address)
            .MaximumLength(500)
            .WithErrorCode(ErrorCodes.General.MAX_LENGTH_EXCEEDED)
            .When(x => !string.IsNullOrWhiteSpace(x.Address));

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .WithErrorCode(ErrorCodes.General.MAX_LENGTH_EXCEEDED)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}