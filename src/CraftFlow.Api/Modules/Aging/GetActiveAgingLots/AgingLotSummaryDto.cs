namespace CraftFlow.Api.Modules.Aging.GetActiveAgingLots;

public record AgingLotSummaryDto(
    Guid LotId,
    string BatchNumber,
    string ChamberName,
    int UnitsCount,
    decimal InitialQuantity,
    int DaysInChamber,
    int TargetDays,
    bool IsReadyForRelease
);