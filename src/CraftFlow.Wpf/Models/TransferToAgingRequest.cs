namespace CraftFlow.Wpf.Models;

public record TransferToAgingRequest(
    Guid ProductionBatchId,
    Guid AgingChamberId,
    int MinAgingDays,
    string? CustomBatchNumber = null
);