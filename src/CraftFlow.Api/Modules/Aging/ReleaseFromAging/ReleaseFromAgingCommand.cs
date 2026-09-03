using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Aging.ReleaseFromAging;

public sealed record ReleaseFromAgingCommand(
    Guid AgingLotId,
    Guid TargetWarehouseId,
    decimal ActualFinalQuantity,
    int UnitsCount,
    decimal UnitPrice
) : IRequest<Result>;