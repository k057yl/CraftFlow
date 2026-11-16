using CraftFlow.Api.Core.Authorization;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.Fermentation.CreateFermentationBatch;

public record CreateFermentationBatchCommand(
    Guid RecipeId,
    Guid WarehouseId,
    Guid DestinationWarehouseId,
    Guid EquipmentLocationId,
    decimal SubstrateWeightKg,
    decimal SaltPercentage,
    decimal TargetTemperature,
    decimal? InitialPh = null,
    string? BatchName = null
) : IRequest<Result<Guid>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.PRODUCTION_EXECUTE;
}