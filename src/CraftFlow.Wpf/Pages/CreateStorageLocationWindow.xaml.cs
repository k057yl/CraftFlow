using CraftFlow.Api.Modules.Aging;
using CraftFlow.Api.Modules.Inventory;
using CraftFlow.SharedKernel.Dtos.Common;
using CraftFlow.Wpf.Services;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace CraftFlow.Wpf.Windows;

public partial class CreateStorageLocationWindow : Window
{
    public CreateStorageLocationWindow(Guid? defaultWarehouseId = null)
    {
        InitializeComponent();
        LoadParentsAsync(defaultWarehouseId);
    }

    private async void LoadParentsAsync(Guid? defaultWarehouseId)
    {
        var warehouses = await ApiService.Instance.GetAsync<List<LookupDto>>(InventoryConstants.WAREHOUSES) ?? [];
        WarehouseComboBox.ItemsSource = warehouses;

        var chambers = await ApiService.Instance.GetAsync<List<LookupDto>>(AgingConstants.AGING_CHAMBERS) ?? [];
        ChamberComboBox.ItemsSource = chambers;

        if (defaultWarehouseId.HasValue && defaultWarehouseId != Guid.Empty)
        {
            WarehouseComboBox.SelectedValue = defaultWarehouseId.Value;
        }
    }

    private void Warehouse_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (WarehouseComboBox.SelectedValue != null) ChamberComboBox.SelectedIndex = -1;
    }

    private void Chamber_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ChamberComboBox.SelectedValue != null) WarehouseComboBox.SelectedIndex = -1;
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        var name = NameTextBox.Text?.Trim();
        var selectedTypeItem = LocationTypeComboBox.SelectedItem as ComboBoxItem;
        var locationType = selectedTypeItem?.Tag?.ToString();

        Guid? warehouseId = WarehouseComboBox.SelectedValue as Guid?;
        Guid? chamberId = ChamberComboBox.SelectedValue as Guid?;

        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(locationType) ||
            (!warehouseId.HasValue && !chamberId.HasValue))
        {
            MessageBox.Show("Заполните название, тип и выберите склад или камеру!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        decimal? capacity = null;
        if (decimal.TryParse(CapacityTextBox.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var cap))
        {
            capacity = cap;
        }

        var (isSuccess, error) = await ApiService.Instance.PostAndReadAsync($"{InventoryConstants.WAREHOUSES}/locations", new
        {
            Name = name,
            LocationType = locationType,
            WarehouseId = warehouseId,
            ChamberId = chamberId,
            Capacity = capacity
        });

        if (isSuccess)
        {
            DialogResult = true;
            Close();
        }
        else
        {
            MessageBox.Show(error, "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}