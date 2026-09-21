using CraftFlow.SharedKernel.Constants;
using FluentValidation;

namespace CraftFlow.Api.Modules.Inventory.CreateStorageLocation;

public class CreateStorageLocationCommandValidator : AbstractValidator<CreateStorageLocationCommand>
{
    public CreateStorageLocationCommandValidator()
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

        RuleFor(x => x.Capacity)
            .GreaterThan(0)
            .When(x => x.Capacity.HasValue)
            .WithErrorCode(ErrorCodes.Inventory.INVALID_CAPACITY);
    }
}