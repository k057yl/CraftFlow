using CraftFlow.Api.Common.Behaviors;
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
) : IRequest<Result<Guid>>, IRequireQuotaValidation
{
    public QuotaType QuotaType => QuotaType.MonthlyBatches;
}