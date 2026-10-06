using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Dtos.Catalog.Nomenclature;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Catalog.GetRecipes;

public class GetRecipesHandler : IRequestHandler<GetRecipesQuery, Result<List<RecipeDto>>>
{
    private readonly AppDbContext _dbContext;

    public GetRecipesHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<RecipeDto>>> Handle(GetRecipesQuery request, CancellationToken cancellationToken)
    {
        var recipes = await _dbContext.Recipes
            .AsNoTracking()
            .Select(r => new RecipeDto(
                r.Id,
                r.ProductId,
                r.Product.Name,
                r.Name,
                r.TargetOutputQuantity,
                r.IsAgingRequired,
                r.DefaultMinAgingDays,
                r.Ingredients.Select(i => new RecipeIngredientDetailDto(
                    i.RawMaterialId,
                    i.RawMaterial.Name,
                    i.RawMaterial.UnitOfMeasure != null ? i.RawMaterial.UnitOfMeasure.Code : string.Empty,
                    i.Quantity
                )).ToList()
            ))
            .ToListAsync(cancellationToken);

        return Result.Success(recipes);
    }
}