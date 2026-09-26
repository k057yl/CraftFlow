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
            .NotEmpty()
            .MustAsync(async (name, ct) =>
                !await dbContext.Suppliers.AnyAsync(s => s.Name == name && s.TenantId == tenantContext.TenantId, ct))
            .WithErrorCode(ErrorCodes.General.ALREADY_EXISTS);

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Phone)
            .Matches(@"^[0-9\+\-\(\)\s]+$")
            .WithMessage(ErrorCodes.Supplier.PHONE_NUMBER_INVALID_FORMAT)
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));
    }
}