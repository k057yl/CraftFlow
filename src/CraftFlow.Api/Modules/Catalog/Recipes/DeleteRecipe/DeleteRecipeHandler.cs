using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Enums.Inventory;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Catalog.Recipes.DeleteRecipe;

public class DeleteRecipeHandler : IRequestHandler<DeleteRecipeCommand, Result>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public DeleteRecipeHandler(AppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(DeleteRecipeCommand request, CancellationToken cancellationToken)
    {
        if (_tenantContext.Role != TenantRole.OWNER && _tenantContext.Role != TenantRole.ACCOUNTANT)
        {
            return Result.Failure(Error.Forbidden(ErrorCodes.Auth.ACCESS_DENIED));
        }

        var hasActiveBatches = await _dbContext.ProductionBatches
            .AnyAsync(b => b.RecipeId == request.Id &&
                          (b.State == BatchState.DRAFT || b.State == BatchState.IN_PROGRESS || b.State == BatchState.IN_AGING),
                      cancellationToken);

        if (hasActiveBatches)
        {
            return Result.Failure(Error.Conflict(ErrorCodes.Catalog.RECIPE_HAS_ACTIVE_BATCHES));
        }

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