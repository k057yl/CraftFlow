using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Catalog.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Catalog.CreateRecipe
{
    public class CreateRecipeHandler : IRequestHandler<CreateRecipeCommand, Result<Guid>>
    {
        private readonly AppDbContext _dbContext;

        public CreateRecipeHandler(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<Guid>> Handle(CreateRecipeCommand request, CancellationToken cancellationToken)
        {
            var productExists = await _dbContext.Products
                .AnyAsync(p => p.Id == request.ProductId, cancellationToken);

            if (!productExists)
            {
                return Result.Failure<Guid>(Error.NotFound(ErrorCodes.General.NOT_FOUND));
            }

            var rawMaterialIds = request.Ingredients.Select(i => i.RawMaterialId).Distinct().ToList();
            var existingRawMaterialsCount = await _dbContext.RawMaterials
                .CountAsync(r => rawMaterialIds.Contains(r.Id), cancellationToken);

            if (existingRawMaterialsCount != rawMaterialIds.Count)
            {
                return Result.Failure<Guid>(Error.NotFound(ErrorCodes.General.NOT_FOUND));
            }

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
}
