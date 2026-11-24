using System.ComponentModel;

namespace CraftFlow.Wpf.Models;

public class CheeseHeadItemModel : INotifyPropertyChanged
{
    private Guid _itemId;
    public Guid ItemId
    {
        get => _itemId;
        set { _itemId = value; OnPropertyChanged(nameof(ItemId)); }
    }

    private string _itemCode = string.Empty;
    public string ItemCode
    {
        get => _itemCode;
        set { _itemCode = value; OnPropertyChanged(nameof(ItemCode)); }
    }

    private decimal _initialWeight;
    public decimal InitialWeight
    {
        get => _initialWeight;
        set
        {
            _initialWeight = value;
            OnPropertyChanged(nameof(InitialWeight));
            OnPropertyChanged(nameof(ShrinkagePercentText));
        }
    }

    private decimal _weight;
    public decimal Weight
    {
        get => _weight;
        set
        {
            _weight = value;
            OnPropertyChanged(nameof(Weight));
            OnPropertyChanged(nameof(ShrinkagePercentText));
        }
    }

    private bool _isDiscarded;
    public bool IsDiscarded
    {
        get => _isDiscarded;
        set { _isDiscarded = value; OnPropertyChanged(nameof(IsDiscarded)); }
    }

    public string ShrinkagePercentText
    {
        get
        {
            if (InitialWeight <= 0 || Weight <= 0) return "—";
            var lossPct = ((InitialWeight - Weight) / InitialWeight) * 100m;
            return $"{lossPct:F1}%";
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}