using CraftFlow.SharedKernel.Constants;
using System.ComponentModel;

namespace CraftFlow.Wpf.Models.Productions;
public record BatchCostDto(
    Guid BatchId,
    string RecipeName,
    decimal PlannedOutputQuantity,
    decimal ActualOutputQuantity,
    decimal TotalRawMaterialCost,
    decimal TotalCostWithOverhead,
    decimal? OverheadPercentage,
    decimal UnitCost
);

public record RequirementCalculationDto(
    string MaterialName,
    decimal RequiredQty,
    decimal AvailableQty,
    bool IsSufficient
)
{
    public string DisplayInfo => string.Format(
        FormattingConstants.DISPLAY_INFO_REQUIREMENT_FORMAT,
        MaterialName,
        Math.Round(RequiredQty, 3),
        Math.Round(AvailableQty, 3),
        IsSufficient ? FormattingConstants.CHECKMARK_SUFFICIENT : FormattingConstants.CHECKMARK_INSUFFICIENT
    );
}

public record BatchReadyForAgingDto(Guid Id, string Name, int DefaultAgingDays);

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

public record GetAgingLotDetailsDto(
    Guid LotId,
    Guid ProductId,
    string ProductName,
    string UnitName,
    decimal TotalBatchCost,
    decimal InitialQuantity,
    int UnitsCount
);

public class ActiveBatchSummaryDto : INotifyPropertyChanged
{
    public Guid Id { get; set; }
    public string BatchName { get; set; } = string.Empty;
    public string RecipeName { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public int TargetDurationMinutes { get; set; }
    public int ElapsedMinutes { get; set; }
    public bool IsOverdue { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal PlannedOutputQuantity { get; set; }

    private TimeSpan _currentElapsed;
    public TimeSpan CurrentElapsed
    {
        get => _currentElapsed;
        set
        {
            _currentElapsed = value;
            OnPropertyChanged(nameof(FormattedElapsed));
            OnPropertyChanged(nameof(IsOverdueStatus));
        }
    }

    public string FormattedElapsed => $"{((int)CurrentElapsed.TotalHours):D2}:{CurrentElapsed.Minutes:D2}:{CurrentElapsed.Seconds:D2}";

    public bool IsOverdueStatus => CurrentElapsed.TotalMinutes >= TargetDurationMinutes;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}