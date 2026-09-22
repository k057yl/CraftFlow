namespace CraftFlow.Api.Modules.Aging.GetActiveAgingLots;

public class AgingLotSummaryDto
{
    public Guid LotId { get; set; }
    public Guid Id { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ChamberName { get; set; } = string.Empty;
    public int UnitsCount { get; set; }
    public decimal InitialQuantity { get; set; }
    public int DaysInChamber { get; set; }
    public int TargetDays { get; set; }
    public bool IsReadyForRelease { get; set; }
}