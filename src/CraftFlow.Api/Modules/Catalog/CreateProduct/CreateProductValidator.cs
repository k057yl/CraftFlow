using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Catalog.CreateProduct;

public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator(AppDbContext dbContext)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.UnitOfMeasureId)
            .NotEmpty()
            .MustAsync(async (unitId, ct) =>
                await dbContext.UnitsOfMeasure.AnyAsync(u => u.Id == unitId, ct))
            .WithErrorCode(ErrorCodes.Catalog.UNIT_OF_MEASURE_NOT_FOUND);
    }
}