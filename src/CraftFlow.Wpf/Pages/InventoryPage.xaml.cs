using CraftFlow.Api.Modules.Aging;
using CraftFlow.Api.Modules.Inventory;
using CraftFlow.SharedKernel.Dtos.Inventory;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Services;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CraftFlow.Wpf.Pages;

public partial class InventoryPage : Page
{
    public ObservableCollection<LookupItem> Warehouses { get; } = [];
    public ObservableCollection<LookupItem> Chambers { get; } = [];
    public ObservableCollection<StorageLocationDto> StorageLocations { get; } = [];

    public InventoryPage()
    {
        InitializeComponent();

        WarehousesDataGrid.ItemsSource = Warehouses;
        ChambersDataGrid.ItemsSource = Chambers;
        StorageLocationsDataGrid.ItemsSource = StorageLocations;

        LocationWarehouseComboBox.ItemsSource = Warehouses;
        LocationChamberComboBox.ItemsSource = Chambers;

        Loaded += async (s, e) => await LoadDataAsync();
    }

    private static bool TryParseDecimal(string text, out decimal result)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            result = 0;
            return false;
        }

        var normalized = text.Trim().Replace('.', ',');
        if (decimal.TryParse(normalized, out result)) return true;

        normalized = text.Trim().Replace(',', '.');
        return decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var warehouses = await ApiService.Instance.GetAsync<List<LookupDto>>(InventoryConstants.WAREHOUSES);
            Warehouses.Clear();
            warehouses?.ForEach(w => Warehouses.Add(new LookupItem(w.Id, w.Name)));

            var chambers = await ApiService.Instance.GetAsync<List<LookupDto>>(AgingConstants.AGING_CHAMBERS);
            Chambers.Clear();
            chambers?.ForEach(c => Chambers.Add(new LookupItem(c.Id, c.Name)));

            var locations = await ApiService.Instance.GetAsync<List<StorageLocationDto>>($"{InventoryConstants.WAREHOUSES}/locations");
            StorageLocations.Clear();
            locations?.ForEach(l => StorageLocations.Add(l));

            SetStatus("UI_DATA_LOADED_SUCCESS", Brushes.Green);
        }
        catch (Exception ex)
        {
            SetStatusFormatted("UI_DATA_LOAD_ERROR", Brushes.Red, ex.Message);
        }
    }

    private async void CreateWarehouse_Click(object sender, RoutedEventArgs e)
    {
        var name = WarehouseNameTextBox.Text?.Trim();
        var address = WarehouseAddressTextBox.Text?.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            SetStatus("UI_INVALID_INPUT_FIELDS", Brushes.Red);
            return;
        }

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(InventoryConstants.WAREHOUSES, new
        {
            Name = name,
            Address = address
        });

        if (isSuccess)
        {
            SetStatus("UI_WAREHOUSE_CREATED_SUCCESS", Brushes.Green);
            WarehouseNameTextBox.Clear();
            WarehouseAddressTextBox.Clear();
            await LoadDataAsync();
        }
        else
        {
            SetStatusRaw(contentOrError, Brushes.Red);
        }
    }

    private async void CreateChamber_Click(object sender, RoutedEventArgs e)
    {
        var name = ChamberNameTextBox.Text?.Trim();

        if (string.IsNullOrWhiteSpace(name) ||
            !TryParseDecimal(ChamberTempTextBox.Text, out var temp) ||
            !TryParseDecimal(ChamberHumidityTextBox.Text, out var humidity))
        {
            SetStatus("UI_INVALID_INPUT_FIELDS", Brushes.Red);
            return;
        }

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(AgingConstants.AGING_CHAMBERS, new
        {
            Name = name,
            TargetTemperature = temp,
            TargetHumidity = humidity
        });

        if (isSuccess)
        {
            SetStatus("UI_AGING_CHAMBER_CREATED_SUCCESS", Brushes.Green);
            ChamberNameTextBox.Clear();
            ChamberTempTextBox.Text = "12.0";
            ChamberHumidityTextBox.Text = "85.0";
            await LoadDataAsync();
        }
        else
        {
            SetStatusRaw(contentOrError, Brushes.Red);
        }
    }

    private async void CreateStorageLocation_Click(object sender, RoutedEventArgs e)
    {
        var name = LocationNameTextBox.Text?.Trim();
        var selectedTypeItem = LocationTypeComboBox.SelectedItem as ComboBoxItem;
        var locationType = selectedTypeItem?.Tag?.ToString();

        Guid? warehouseId = LocationWarehouseComboBox.SelectedValue as Guid?;
        Guid? chamberId = LocationChamberComboBox.SelectedValue as Guid?;

        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(locationType) ||
            (!warehouseId.HasValue && !chamberId.HasValue))
        {
            SetStatus("UI_INVALID_INPUT_FIELDS", Brushes.Red);
            return;
        }

        decimal? capacity = TryParseDecimal(LocationCapacityTextBox.Text, out var cap) ? cap : null;

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync($"{InventoryConstants.WAREHOUSES}/locations", new
        {
            Name = name,
            LocationType = locationType,
            WarehouseId = warehouseId,
            ChamberId = chamberId,
            Capacity = capacity
        });

        if (isSuccess)
        {
            SetStatus("UI_DATA_LOADED_SUCCESS", Brushes.Green);
            LocationNameTextBox.Clear();
            LocationCapacityTextBox.Clear();
            LocationWarehouseComboBox.SelectedIndex = -1;
            LocationChamberComboBox.SelectedIndex = -1;
            await LoadDataAsync();
        }
        else
        {
            SetStatusRaw(contentOrError, Brushes.Red);
        }
    }

    private async void DeleteWarehouse_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Guid id)
        {
            var (isSuccess, error) = await ApiService.Instance.DeleteAndReadAsync($"{InventoryConstants.WAREHOUSES}/{id}");
            if (isSuccess)
            {
                SetStatus("UI_DATA_LOADED_SUCCESS", Brushes.Green);
                await LoadDataAsync();
            }
            else
            {
                SetStatusRaw(error, Brushes.Red);
            }
        }
    }

    private async void DeleteChamber_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Guid id)
        {
            var (isSuccess, error) = await ApiService.Instance.DeleteAndReadAsync($"{AgingConstants.AGING_CHAMBERS}/{id}");
            if (isSuccess)
            {
                SetStatus("UI_DATA_LOADED_SUCCESS", Brushes.Green);
                await LoadDataAsync();
            }
            else
            {
                SetStatusRaw(error, Brushes.Red);
            }
        }
    }

    private async void DeleteStorageLocation_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Guid id)
        {
            var (isSuccess, error) = await ApiService.Instance.DeleteAndReadAsync($"{InventoryConstants.WAREHOUSES}/locations/{id}");
            if (isSuccess)
            {
                SetStatus("UI_DATA_LOADED_SUCCESS", Brushes.Green);
                await LoadDataAsync();
            }
            else
            {
                SetStatusRaw(error, Brushes.Red);
            }
        }
    }

    private void SetStatus(string resourceKey, Brush color)
    {
        StatusTextBlock.Foreground = color;
        StatusTextBlock.Text = LocalizationService.Get(resourceKey);
    }

    private void SetStatusFormatted(string resourceKey, Brush color, params object[] args)
    {
        StatusTextBlock.Foreground = color;
        var format = LocalizationService.Get(resourceKey);
        StatusTextBlock.Text = string.Format(format, args);
    }

    private void SetStatusRaw(string rawText, Brush color)
    {
        StatusTextBlock.Foreground = color;
        StatusTextBlock.Text = rawText;
    }
}