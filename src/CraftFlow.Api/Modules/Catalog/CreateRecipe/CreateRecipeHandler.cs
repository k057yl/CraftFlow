using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Catalog.Domain;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Catalog.CreateRecipe;

public class CreateRecipeHandler : IRequestHandler<CreateRecipeCommand, Result<Guid>>
{
    private readonly AppDbContext _dbContext;

    public CreateRecipeHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(CreateRecipeCommand request, CancellationToken cancellationToken)
    {
        var recipe = Recipe.Create(
            request.ProductId,
            request.Name,
            request.TargetOutputQuantity,
            request.IsAgingRequired,
            request.DefaultMinAgingDays
        );

        foreach (var ingredient in request.Ingredients)
        {
            recipe.AddIngredient(ingredient.RawMaterialId, ingredient.Quantity);
        }

        _dbContext.Recipes.Add(recipe);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(recipe.Id);
    }
}