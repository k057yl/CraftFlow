using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using CraftFlow.SharedKernel.Dtos.Aging;

namespace CraftFlow.Wpf.Pages;

public partial class WriteOffDialog : Window
{
    public class SelectableLotItem : INotifyPropertyChanged
    {
        private bool _isSelected;

        public Guid Id { get; set; }
        public string ItemNumber { get; set; } = string.Empty;
        public decimal CurrentWeight { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public string DisplayInfo => $"{ItemNumber} ({CurrentWeight:F2} кг)";

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    private readonly decimal _maxQuantity;
    public ObservableCollection<SelectableLotItem> LotItems { get; } = [];

    public decimal QuantityToWriteOff { get; private set; }
    public List<Guid> SelectedItemIds { get; private set; } = [];
    public string Reason { get; private set; } = string.Empty;

    public WriteOffDialog(string batchNumber, string locationOrChamber, decimal currentQuantity)
        : this(batchNumber, locationOrChamber, currentQuantity, items: null)
    {
    }

    public WriteOffDialog(string batchNumber, string chamberName, decimal currentQuantity, List<GetAgingLotItemDto>? items)
    {
        InitializeComponent();

        _maxQuantity = currentQuantity;
        LotTitleTextBlock.Text = $"{batchNumber} ({chamberName})";
        CurrentQuantityTextBlock.Text = $"Доступный остаток: {currentQuantity:N2} кг" +
            (items != null && items.Count > 0 ? $" | Головок: {items.Count} шт." : string.Empty);

        if (items == null || items.Count == 0)
        {
            ItemsGroupBox.Visibility = Visibility.Collapsed;
            Height = 360;
        }
        else
        {
            foreach (var item in items.Where(i => i.State == 1))
            {
                var selectableItem = new SelectableLotItem
                {
                    Id = item.Id,
                    ItemNumber = item.ItemNumber,
                    CurrentWeight = item.CurrentWeight,
                    IsSelected = false
                };

                selectableItem.PropertyChanged += Item_PropertyChanged;
                LotItems.Add(selectableItem);
            }
        }

        ItemsListBox.ItemsSource = LotItems;
        QuantityTextBox.Text = "0.00";
        UpdateSelectedSummary();
    }

    private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectableLotItem.IsSelected))
        {
            UpdateSelectedSummary();
        }
    }

    private void UpdateSelectedSummary()
    {
        var selectedItems = LotItems.Where(i => i.IsSelected).ToList();

        SelectedCountTextBox.Text = $"{selectedItems.Count} шт.";

        if (selectedItems.Count > 0)
        {
            decimal totalWeight = selectedItems.Sum(i => i.CurrentWeight);
            QuantityTextBox.Text = totalWeight.ToString("F2", CultureInfo.InvariantCulture);
        }
    }

    private static bool TryParseDecimal(string text, out decimal result)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            result = 0m;
            return false;
        }

        var normalized = text.Trim().Replace(',', '.');
        return decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
    }

    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        if (!TryParseDecimal(QuantityTextBox.Text, out var qty) || qty < 0)
        {
            MessageBox.Show("Введите корректный вес для списания!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var selected = LotItems.Where(i => i.IsSelected).ToList();
        SelectedItemIds = selected.Select(i => i.Id).ToList();

        if (qty <= 0 && SelectedItemIds.Count == 0)
        {
            MessageBox.Show("Укажите списываемый вес (усушку) или выберите конкретные головки!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (qty > _maxQuantity)
        {
            MessageBox.Show($"Списываемый вес ({qty:N2} кг) превышает доступный остаток ({_maxQuantity:N2} кг)!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        QuantityToWriteOff = qty;
        Reason = string.IsNullOrWhiteSpace(ReasonTextBox.Text) ? "Технические потери / Усушка" : ReasonTextBox.Text.Trim();

        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}