using System.ComponentModel;

namespace CraftFlow.Wpf.Models.Sales;

public sealed class InvoiceItemRow : INotifyPropertyChanged
{
    public Guid StockLotId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal AvailableQuantity { get; set; }

    private decimal _quantity;
    public decimal Quantity
    {
        get => _quantity;
        set
        {
            _quantity = Math.Min(value, AvailableQuantity);
            if (_quantity < 0) _quantity = 0;
            OnPropertyChanged(nameof(Quantity));
            OnPropertyChanged(nameof(TotalSum));
        }
    }

    private decimal _sellingPrice;
    public decimal SellingPrice
    {
        get => _sellingPrice;
        set
        {
            _sellingPrice = value < 0 ? 0 : value;
            OnPropertyChanged(nameof(SellingPrice));
            OnPropertyChanged(nameof(TotalSum));
        }
    }

    public decimal TotalSum => Quantity * SellingPrice;

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}