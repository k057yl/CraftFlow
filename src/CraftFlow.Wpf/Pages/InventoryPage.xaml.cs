using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Services;

namespace CraftFlow.Wpf.Pages;

public partial class InventoryPage : Page
{
    public ObservableCollection<LookupItem> Warehouses { get; } = [];
    public ObservableCollection<LookupItem> RawMaterials { get; } = [];

    public InventoryPage()
    {
        InitializeComponent();

        StockWarehouseComboBox.ItemsSource = Warehouses;
        StockRawMaterialComboBox.ItemsSource = RawMaterials;

        Loaded += async (s, e) => await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var warehouses = await ApiService.Instance.GetAsync<List<LookupDto>>(Endpoints.WAREHOUSES);
            Warehouses.Clear();
            warehouses?.ForEach(w => Warehouses.Add(new LookupItem(w.Id, w.Name)));

            var raw = await ApiService.Instance.GetAsync<List<LookupDto>>(Endpoints.RAW_MATERIALS);
            RawMaterials.Clear();
            raw?.ForEach(r => RawMaterials.Add(new LookupItem(r.Id, r.Name)));

            SetStatus(UiConstants.Messages.DATA_LOADED_SUCCESS, Brushes.Green);
        }
        catch (Exception ex)
        {
            SetStatus($"{UiConstants.Messages.DATA_LOAD_ERROR}: {ex.Message}", Brushes.Red);
        }
    }

    private async void CreateWarehouse_Click(object sender, RoutedEventArgs e)
    {
        var response = await ApiService.Instance.PostAsync(Endpoints.WAREHOUSES, new
        {
            Name = WarehouseNameTextBox.Text,
            Address = WarehouseAddressTextBox.Text
        });

        if (response.IsSuccessStatusCode)
        {
            SetStatus(UiConstants.Messages.WAREHOUSE_CREATED_SUCCESS, Brushes.Green);
            await LoadDataAsync();
        }
        else
        {
            SetStatus($"{UiConstants.Messages.API_ERROR_PREFIX}: {response.StatusCode}", Brushes.Red);
        }
    }

    private async void AddStockLot_Click(object sender, RoutedEventArgs e)
    {
        if (StockWarehouseComboBox.SelectedValue is not Guid warehouseId ||
            StockRawMaterialComboBox.SelectedValue is not Guid rawId ||
            !decimal.TryParse(StockQuantityTextBox.Text, out var quantity))
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
            return;
        }

        var response = await ApiService.Instance.PostAsync(Endpoints.STOCK_LOTS, new
        {
            WarehouseId = warehouseId,
            ItemId = rawId,
            Quantity = quantity,
            BatchNumber = StockBatchTextBox.Text
        });

        if (response.IsSuccessStatusCode)
        {
            var lotId = await response.Content.ReadFromJsonAsync<Guid>();
            SetStatus($"{UiConstants.Messages.STOCK_LOT_CREATED_SUCCESS} ID: {lotId}", Brushes.Green);
        }
        else
        {
            SetStatus($"{UiConstants.Messages.API_ERROR_PREFIX}: {response.StatusCode}", Brushes.Red);
        }
    }

    private void SetStatus(string msg, Brush color)
    {
        StatusTextBlock.Foreground = color;
        StatusTextBlock.Text = msg;
    }
}