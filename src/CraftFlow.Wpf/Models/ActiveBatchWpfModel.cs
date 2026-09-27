using System.ComponentModel;

namespace CraftFlow.Wpf.Models;

public class ActiveBatchWpfModel : INotifyPropertyChanged
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