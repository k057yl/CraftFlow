using System.ComponentModel;

namespace CraftFlow.Wpf.Models;
public class CheeseHeadItemModel : INotifyPropertyChanged
{
    private string _itemCode = string.Empty;
    public string ItemCode
    {
        get => _itemCode;
        set { _itemCode = value; OnPropertyChanged(nameof(ItemCode)); }
    }

    private decimal _weight;
    public decimal Weight
    {
        get => _weight;
        set { _weight = value; OnPropertyChanged(nameof(Weight)); }
    }

    private bool _isDiscarded;
    public bool IsDiscarded
    {
        get => _isDiscarded;
        set { _isDiscarded = value; OnPropertyChanged(nameof(IsDiscarded)); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}