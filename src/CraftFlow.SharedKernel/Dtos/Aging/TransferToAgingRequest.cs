namespace CraftFlow.SharedKernel.Dtos.Aging;

public record TransferToAgingRequest(
    Guid ProductionBatchId,
    Guid AgingChamberId,
    int MinAgingDays,
    int UnitsCount,
    string? CustomBatchNumber
);