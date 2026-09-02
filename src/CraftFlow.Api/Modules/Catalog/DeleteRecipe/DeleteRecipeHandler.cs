using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Catalog.DeleteRecipe;

public class DeleteRecipeHandler : IRequestHandler<DeleteRecipeCommand, Result>
{
    private readonly AppDbContext _dbContext;

    public DeleteRecipeHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(DeleteRecipeCommand request, CancellationToken cancellationToken)
    {
        var recipe = await _dbContext.Recipes
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (recipe is null)
        {
            return Result.Failure(Error.NotFound(ErrorCodes.General.NOT_FOUND));
        }

        recipe.Archive();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}