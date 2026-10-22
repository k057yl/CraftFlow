using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Catalog.RawMaterials.DeleteRawMaterial;

public class DeleteRawMaterialHandler : IRequestHandler<DeleteRawMaterialCommand, Result>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public DeleteRawMaterialHandler(AppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(DeleteRawMaterialCommand request, CancellationToken cancellationToken)
    {
        if (_tenantContext.Role != TenantRole.OWNER && _tenantContext.Role != TenantRole.ACCOUNTANT)
        {
            return Result.Failure(Error.Forbidden(ErrorCodes.Auth.ACCESS_DENIED));
        }

        var hasStock = await _dbContext.StockLots
            .AnyAsync(s => s.ItemId == request.Id && s.Quantity > 0, cancellationToken);

        if (hasStock)
        {
            return Result.Failure(Error.Conflict(ErrorCodes.Catalog.RAW_MATERIAL_HAS_STOCK));
        }

        var usedInActiveRecipes = await _dbContext.RecipeIngredients
            .AnyAsync(ri => ri.RawMaterialId == request.Id && ri.Recipe.IsActive, cancellationToken);

        if (usedInActiveRecipes)
        {
            return Result.Failure(Error.Conflict(ErrorCodes.Catalog.RAW_MATERIAL_IN_ACTIVE_RECIPE));
        }

        var rowsAffected = await _dbContext.RawMaterials
            .Where(r => r.Id == request.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.IsActive, false), cancellationToken);

        if (rowsAffected == 0)
        {
            return Result.Failure(Error.NotFound(ErrorCodes.General.NOT_FOUND));
        }

        return Result.Success();
    }
}