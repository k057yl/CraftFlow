using System.ComponentModel;

namespace CraftFlow.Wpf.Models;

public class ActiveBatchWpfModel : INotifyPropertyChanged
{
    public Guid Id { get; set; }
    public string BatchName { get; set; } = string.Empty;
    public string RecipeName { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public int TargetDurationMinutes { get; set; }
    public int ElapsedMinutes { get; set; }
    public bool IsOverdue { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal PlannedOutputQuantity { get; set; }
    public bool IsDraft { get; set; }

    private TimeSpan _currentElapsed;
    public TimeSpan CurrentElapsed
    {
        get => _currentElapsed;
        set
        {
            _currentElapsed = value;
            OnPropertyChanged(nameof(FormattedElapsed));
            OnPropertyChanged(nameof(FormattedCountdown));
            OnPropertyChanged(nameof(IsOverdueStatus));
        }
    }

    public string FormattedElapsed => StartedAt.HasValue
        ? $"{((int)CurrentElapsed.TotalHours):D2}:{CurrentElapsed.Minutes:D2}:{CurrentElapsed.Seconds:D2}"
        : "—";

    public string FormattedCountdown
    {
        get
        {
            if (!ScheduledAt.HasValue) return "—";

            var scheduledUtc = ScheduledAt.Value.Kind == DateTimeKind.Utc
                ? ScheduledAt.Value
                : ScheduledAt.Value.ToUniversalTime();

            var remaining = scheduledUtc - DateTime.UtcNow;
            if (remaining <= TimeSpan.Zero) return "00:00:00";

            return $"{((int)remaining.TotalHours):D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
        }
    }

    public bool IsOverdueStatus => !IsDraft && StartedAt.HasValue && CurrentElapsed.TotalMinutes >= TargetDurationMinutes;

    public void NotifyCountdownChanged()
    {
        OnPropertyChanged(nameof(FormattedCountdown));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}