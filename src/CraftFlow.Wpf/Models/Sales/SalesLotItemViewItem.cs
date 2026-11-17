using System.ComponentModel;

namespace CraftFlow.Wpf.Models.Sales;
public sealed class SalesLotItemViewItem : INotifyPropertyChanged
{
    public Guid Id { get; set; }
    public string ItemNumber { get; set; } = string.Empty;
    public decimal CurrentWeight { get; set; }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}