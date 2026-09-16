namespace CraftFlow.Api.Modules.Production.GetBatchesReadyForAging;

public record BatchReadyForAgingDto(
    Guid Id,
    string Name,
    int DefaultAgingDays
);