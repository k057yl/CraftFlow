using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Production.StartProductionBatch
{
    public record StartProductionBatchCommand(
        Guid RecipeId,
        Guid WarehouseId,
        decimal PlannedOutputQuantity
    ) : IRequest<Result<Guid>>;
}
