using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Aging.CreateChamber;

public class CreateChamberValidator : AbstractValidator<CreateChamberCommand>
{
    public CreateChamberValidator(AppDbContext dbContext, ITenantContext tenantContext)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Aging.CHAMBER_NAME_REQUIRED)
            .MustAsync(async (name, ct) =>
            {
                return !await dbContext.AgingChambers.AnyAsync(c => c.Name == name && c.TenantId == tenantContext.TenantId, ct);
            })
            .WithErrorCode(ErrorCodes.General.ALREADY_EXISTS);

        RuleFor(x => x.TargetTemperature)
            .InclusiveBetween(-10m, 35m)
            .WithErrorCode(ErrorCodes.General.INVALID_FORMAT);

        RuleFor(x => x.TargetHumidity)
            .InclusiveBetween(0m, 100m)
            .WithErrorCode(ErrorCodes.General.INVALID_FORMAT);
    }
}