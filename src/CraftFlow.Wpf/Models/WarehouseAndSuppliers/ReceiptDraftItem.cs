using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CraftFlow.Wpf.Models.WarehouseAndSuppliers;

public sealed class ReceiptDraftItem : INotifyPropertyChanged
{
    private decimal _quantity;
    private decimal _unitPrice;
    private string _batchNumber = string.Empty;

    public Guid ItemId { get; init; }
    public string ItemName { get; init; } = string.Empty;
    public string UnitOfMeasure { get; init; } = string.Empty;
    public string UnitOfMeasureDisplay { get; init; } = string.Empty;
    public DateTime? ExpirationDate { get; set; }
    public List<Guid> StorageLocationIds { get; init; } = [];
    public string StorageLocationsDisplay { get; init; } = "—";

    public decimal Quantity
    {
        get => _quantity;
        set
        {
            if (_quantity != value)
            {
                _quantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LineTotal));
            }
        }
    }

    public decimal UnitPrice
    {
        get => _unitPrice;
        set
        {
            if (_unitPrice != value)
            {
                _unitPrice = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LineTotal));
            }
        }
    }

    public string BatchNumber
    {
        get => _batchNumber;
        set
        {
            if (_batchNumber != value)
            {
                _batchNumber = value;
                OnPropertyChanged();
            }
        }
    }

    public decimal LineTotal => Math.Round(Quantity * UnitPrice, 2);

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}