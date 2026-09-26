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
        var rowsAffected = await _dbContext.Recipes
            .Where(r => r.Id == request.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.IsActive, false), cancellationToken);

        if (rowsAffected == 0)
        {
            return Result.Failure(Error.NotFound(ErrorCodes.General.NOT_FOUND));
        }

        return Result.Success();
    }
}