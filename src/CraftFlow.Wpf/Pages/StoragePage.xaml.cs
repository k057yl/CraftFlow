using CraftFlow.Api.Modules.Aging;
using CraftFlow.Api.Modules.Inventory;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Dtos.Common;
using CraftFlow.SharedKernel.Dtos.Inventory;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Services;
using CraftFlow.Wpf.Windows;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CraftFlow.Wpf.Pages;

public partial class StoragePage : Page
{
    public ObservableCollection<LookupItem> Warehouses { get; } = [];
    public ObservableCollection<LookupItem> Chambers { get; } = [];
    public ObservableCollection<StorageLocationViewItem> StorageLocations { get; } = [];

    public StoragePage()
    {
        InitializeComponent();

        WarehousesDataGrid.ItemsSource = Warehouses;
        ChambersDataGrid.ItemsSource = Chambers;
        StorageLocationsDataGrid.ItemsSource = StorageLocations;

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
            locations?.ForEach(l =>
            {
                string parentName = !string.IsNullOrWhiteSpace(l.WarehouseName)
                    ? $"🏠 {l.WarehouseName}"
                    : (!string.IsNullOrWhiteSpace(l.ChamberName) ? $"❄ {l.ChamberName}" : "—");

                StorageLocations.Add(new StorageLocationViewItem
                {
                    Id = l.Id,
                    Name = l.Name,
                    LocationType = l.LocationType,
                    VolumeInfo = l.VolumeInfo,
                    ParentName = parentName
                });
            });

            SetStatus(UiConstants.Messages.DATA_LOADED_SUCCESS, Brushes.Green);
        }
        catch (Exception ex)
        {
            SetStatusFormatted(UiConstants.Messages.DATA_LOAD_ERROR, Brushes.Red, ex.Message);
        }
    }

    private async void CreateWarehouse_Click(object sender, RoutedEventArgs e)
    {
        var name = WarehouseNameTextBox.Text?.Trim();
        var address = WarehouseAddressTextBox.Text?.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
            return;
        }

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(InventoryConstants.WAREHOUSES, new
        {
            Name = name,
            Address = address
        });

        if (isSuccess)
        {
            SetStatus(UiConstants.Messages.WAREHOUSE_CREATED_SUCCESS, Brushes.Green);
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
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
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
            SetStatus(UiConstants.Messages.AGING_CHAMBER_CREATED_SUCCESS, Brushes.Green);
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

    private async void OpenCreateStorageLocationModal_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new CreateStorageLocationWindow
        {
            Owner = Window.GetWindow(this)
        };

        if (dialog.ShowDialog() == true)
        {
            await LoadDataAsync();
            SetStatus(UiConstants.Messages.DATA_LOADED_SUCCESS, Brushes.Green);
        }
    }

    private async void DeleteWarehouse_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Guid id)
        {
            var (isSuccess, error) = await ApiService.Instance.DeleteAndReadAsync($"{InventoryConstants.WAREHOUSES}/{id}");
            if (isSuccess)
            {
                SetStatus(UiConstants.Messages.DATA_LOADED_SUCCESS, Brushes.Green);
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
                SetStatus(UiConstants.Messages.DATA_LOADED_SUCCESS, Brushes.Green);
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
                SetStatus(UiConstants.Messages.DATA_LOADED_SUCCESS, Brushes.Green);
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

public class StorageLocationViewItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LocationType { get; set; } = string.Empty;
    public string ParentName { get; set; } = string.Empty;
    public string VolumeInfo { get; set; } = string.Empty;
}