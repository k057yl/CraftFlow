using FluentValidation;

namespace CraftFlow.Api.Modules.Catalog.CreateRecipe
{
    public class CreateRecipeValidator : AbstractValidator<CreateRecipeCommand>
    {
        public CreateRecipeValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty();

            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.TargetOutputQuantity)
                .GreaterThan(0);

            RuleFor(x => x.Ingredients)
                .NotEmpty();

            RuleForEach(x => x.Ingredients).ChildRules(ingredient =>
            {
                ingredient.RuleFor(i => i.RawMaterialId).NotEmpty();
                ingredient.RuleFor(i => i.Quantity).GreaterThan(0);
            });
        }
    }
}
