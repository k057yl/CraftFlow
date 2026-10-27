using CraftFlow.Api.Common.Authorization;
using CraftFlow.Api.Common.Behaviors;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Production.Batches.StartProductionBatch;

public record StartProductionBatchCommand(
    Guid RecipeId,
    Guid WarehouseId,
    Guid DestinationWarehouseId,
    decimal PlannedOutputQuantity,
    Guid EquipmentLocationId,
    string? Name = null
) : IRequest<Result<Guid>>, IRequireQuotaValidation, IRequirePermission
{
    public QuotaType QuotaType => QuotaType.MonthlyBatches;
    public AppPermission RequiredPermission => AppPermission.PRODUCTION_EXECUTE;
}