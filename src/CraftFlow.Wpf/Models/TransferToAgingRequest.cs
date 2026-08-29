namespace CraftFlow.Wpf.Models;
public sealed record TransferToAgingRequest(Guid ProductionBatchId, Guid AgingChamberId, int MinAgingDays);