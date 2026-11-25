using CraftFlow.Api.Core.Authorization;
using CraftFlow.Api.Core.Behaviors;
using CraftFlow.Api.Core.Enums;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Batches.StartProductionBatch;

public record StartCheeseBatchCommand(
    Guid RecipeId,
    Guid WarehouseId,
    Guid DestinationWarehouseId,
    decimal PlannedOutputQuantity,
    Guid EquipmentLocationId,
    string? Name = null
) : IRequest<Result<Guid>>, IRequireQuotaValidation, IRequirePermission
{
    public QuotaType QuotaType => QuotaType.MONTHLY_BATCHES;
    public AppPermission RequiredPermission => AppPermission.PRODUCTION_EXECUTE;
}