using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Aging.TransferToAging;

public sealed record TransferToAgingCommand(
    Guid ProductionBatchId,
    Guid AgingChamberId,
    int MinAgingDays
) : IRequest<Result<Guid>>;