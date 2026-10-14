using CraftFlow.Api.Modules.Aging;
using CraftFlow.Api.Modules.Inventory;
using CraftFlow.SharedKernel.Dtos.Aging;
using CraftFlow.SharedKernel.Dtos.Common;
using CraftFlow.SharedKernel.Enums.Inventory;
using CraftFlow.Wpf.Services;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace CraftFlow.Wpf.Windows;

public partial class CreateStorageLocationWindow : Window
{
    private readonly Guid? _defaultWarehouseId;

    public CreateStorageLocationWindow(Guid? defaultWarehouseId = null)
    {
        InitializeComponent();
        _defaultWarehouseId = defaultWarehouseId;
        Loaded += async (s, e) => await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var warehouses = await ApiService.Instance.GetAsync<List<LookupDto>>(InventoryConstants.WAREHOUSES) ?? [];
            WarehouseComboBox.ItemsSource = warehouses;

            if (_defaultWarehouseId.HasValue && _defaultWarehouseId.Value != Guid.Empty)
            {
                WarehouseComboBox.SelectedValue = _defaultWarehouseId.Value;
            }

            var chambers = await ApiService.Instance.GetAsync<List<AgingChamberDto>>(AgingConstants.AGING_CHAMBERS) ?? [];
            ChamberComboBox.ItemsSource = chambers;
        }
        catch (Exception ex)
        {
            AlertWindow.ShowError(this, ex.Message);
        }
    }

    private void Warehouse_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (WarehouseComboBox.SelectedValue != null)
        {
            ChamberComboBox.SelectedValue = null;
        }
    }

    private void Chamber_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ChamberComboBox.SelectedValue != null)
        {
            WarehouseComboBox.SelectedValue = null;
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        var name = NameTextBox.Text.Trim();
        var selectedTypeItem = LocationTypeComboBox.SelectedItem as ComboBoxItem;
        var rawTypeStr = selectedTypeItem?.Tag?.ToString();

        var locationType = Enum.TryParse<StorageLocationType>(rawTypeStr, out var parsedType)
            ? parsedType
            : StorageLocationType.CONTAINER;

        Guid? warehouseId = WarehouseComboBox.SelectedValue as Guid?;
        Guid? chamberId = ChamberComboBox.SelectedValue as Guid?;

        if (string.IsNullOrWhiteSpace(name) || (!warehouseId.HasValue && !chamberId.HasValue))
        {
            AlertWindow.ShowError(this, "Заполните название и выберите склад или камеру.");
            return;
        }

        decimal? capacity = null;
        var rawCap = CapacityTextBox.Text.Replace(',', '.');
        if (decimal.TryParse(rawCap, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedCap) && parsedCap > 0)
        {
            capacity = parsedCap;
        }

        var selectedUnitItem = UnitOfMeasureComboBox.SelectedItem as ComboBoxItem;
        var unitOfMeasure = selectedUnitItem?.Tag?.ToString() ?? "l";

        var payload = new
        {
            Name = name,
            LocationType = locationType,
            WarehouseId = warehouseId,
            ChamberId = chamberId,
            Capacity = capacity,
            UnitOfMeasure = unitOfMeasure
        };

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync($"{InventoryConstants.WAREHOUSES}/locations", payload);

        if (isSuccess)
        {
            DialogResult = true;
        }
        else
        {
            AlertWindow.ShowError(this, contentOrError);
        }
    }
}