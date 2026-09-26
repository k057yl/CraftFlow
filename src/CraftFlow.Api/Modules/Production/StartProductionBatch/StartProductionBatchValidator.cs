using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Production.StartProductionBatch;

public class StartProductionBatchValidator : AbstractValidator<StartProductionBatchCommand>
{
    public StartProductionBatchValidator(AppDbContext dbContext)
    {
        RuleFor(x => x.RecipeId)
            .NotEmpty()
            .MustAsync(async (recipeId, ct) =>
                await dbContext.Recipes.AnyAsync(r => r.Id == recipeId, ct))
            .WithErrorCode(ErrorCodes.General.NOT_FOUND);

        RuleFor(x => x.WarehouseId)
            .NotEmpty()
            .MustAsync(async (warehouseId, ct) =>
                await dbContext.Warehouses.AnyAsync(w => w.Id == warehouseId, ct))
            .WithErrorCode(ErrorCodes.Inventory.WAREHOUSE_NOT_FOUND);

        RuleFor(x => x.PlannedOutputQuantity)
            .GreaterThan(0);
    }
}