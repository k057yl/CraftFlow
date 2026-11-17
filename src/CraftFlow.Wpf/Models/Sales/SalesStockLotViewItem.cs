using System.Collections.ObjectModel;
using System.ComponentModel;

namespace CraftFlow.Wpf.Models.Sales;
public sealed class SalesStockLotViewItem : INotifyPropertyChanged
{
    public Guid Id { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public int UnitsCount { get; set; }

    public ObservableCollection<SalesLotItemViewItem> Items { get; set; } = [];

    public event PropertyChangedEventHandler? PropertyChanged;
}