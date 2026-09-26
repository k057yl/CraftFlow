using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Inventory.CreateStorageLocation;

public class CreateStorageLocationValidator : AbstractValidator<CreateStorageLocationCommand>
{
    public CreateStorageLocationValidator(AppDbContext dbContext)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Inventory.LOCATION_NAME_REQUIRED)
            .MaximumLength(200)
            .WithErrorCode(ErrorCodes.General.MAX_LENGTH_EXCEEDED);

        RuleFor(x => x.LocationType)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Inventory.LOCATION_TYPE_REQUIRED)
            .MaximumLength(50)
            .WithErrorCode(ErrorCodes.General.MAX_LENGTH_EXCEEDED);

        RuleFor(x => x)
            .Must(x => x.WarehouseId.HasValue || x.ChamberId.HasValue)
            .WithErrorCode(ErrorCodes.Inventory.PARENT_CONTAINER_REQUIRED);

        RuleFor(x => x.WarehouseId)
            .MustAsync(async (warehouseId, ct) =>
                !warehouseId.HasValue || await dbContext.Warehouses.AnyAsync(w => w.Id == warehouseId.Value, ct))
            .WithErrorCode(ErrorCodes.Inventory.WAREHOUSE_NOT_FOUND);

        RuleFor(x => x.ChamberId)
            .MustAsync(async (chamberId, ct) =>
                !chamberId.HasValue || await dbContext.AgingChambers.AnyAsync(c => c.Id == chamberId.Value, ct))
            .WithErrorCode(ErrorCodes.Aging.CHAMBER_NOT_FOUND);

        RuleFor(x => x.Capacity)
            .GreaterThan(0)
            .When(x => x.Capacity.HasValue)
            .WithErrorCode(ErrorCodes.Inventory.INVALID_CAPACITY);
    }
}