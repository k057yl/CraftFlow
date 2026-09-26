using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Catalog.CreateRecipe;

public class CreateRecipeValidator : AbstractValidator<CreateRecipeCommand>
{
    public CreateRecipeValidator(AppDbContext dbContext)
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .MustAsync(async (productId, ct) =>
                await dbContext.Products.AnyAsync(p => p.Id == productId, ct))
            .WithErrorCode(ErrorCodes.General.NOT_FOUND);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.TargetOutputQuantity)
            .GreaterThan(0);

        RuleFor(x => x.Ingredients)
            .NotEmpty()
            .MustAsync(async (ingredients, ct) =>
            {
                if (ingredients == null || ingredients.Count == 0) return false;

                var rawMaterialIds = ingredients.Select(i => i.RawMaterialId).Distinct().ToList();
                var count = await dbContext.RawMaterials
                    .CountAsync(r => rawMaterialIds.Contains(r.Id), ct);

                return count == rawMaterialIds.Count;
            })
            .WithErrorCode(ErrorCodes.General.NOT_FOUND);

        RuleForEach(x => x.Ingredients).ChildRules(ingredient =>
        {
            ingredient.RuleFor(i => i.RawMaterialId).NotEmpty();
            ingredient.RuleFor(i => i.Quantity).GreaterThan(0);
        });
    }
}