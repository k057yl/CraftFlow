using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Aging.Domain;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Aging.DeleteChamber;
public class DeleteChamberValidator : AbstractValidator<DeleteChamberCommand>
{
    public DeleteChamberValidator(AppDbContext dbContext)
    {
        RuleFor(x => x.Id)
            .MustAsync(async (id, ct) =>
            {
                var hasActiveLots = await dbContext.AgingLots
                    .AnyAsync(l => l.AgingChamberId == id && l.State == AgingState.InChamber, ct);
                return !hasActiveLots;
            })
            .WithErrorCode("CHAMBER_HAS_ACTIVE_LOTS");
    }
}
