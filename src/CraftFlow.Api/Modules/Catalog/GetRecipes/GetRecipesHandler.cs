using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Catalog.GetRecipes
{
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
                .Select(r => new RecipeDto(r.Id, r.Name))
                .ToListAsync(cancellationToken);

            return Result.Success(recipes);
        }
    }
}
