using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Aging.TransferToAging;

public record TransferToAgingRequest(
    Guid ProductionBatchId,
    Guid AgingChamberId,
    int MinAgingDays,
    string? CustomBatchNumber = null
);

public record TransferToAgingCommand(
    Guid ProductionBatchId,
    Guid AgingChamberId,
    int MinAgingDays,
    string? CustomBatchNumber = null
) : IRequest<Result<Guid>>;