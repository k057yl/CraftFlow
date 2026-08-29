using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Services;

namespace CraftFlow.Wpf.Pages;

public partial class ProcurementPage : Page
{
    public ObservableCollection<LookupItem> Suppliers { get; } = [];
    public ObservableCollection<LookupItem> Warehouses { get; } = [];
    public ObservableCollection<LookupItem> RawMaterials { get; } = [];

    public ProcurementPage()
    {
        InitializeComponent();

        StockSupplierComboBox.ItemsSource = Suppliers;
        StockWarehouseComboBox.ItemsSource = Warehouses;
        StockRawMaterialComboBox.ItemsSource = RawMaterials;

        Loaded += async (s, e) => await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var suppliers = await ApiService.Instance.GetAsync<List<LookupDto>>(Endpoints.SUPPLIERS);
            Suppliers.Clear();
            suppliers?.ForEach(s => Suppliers.Add(new LookupItem(s.Id, s.Name)));

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

    private async void CreateSupplier_Click(object sender, RoutedEventArgs e)
    {
        var name = NameTextBox.Text.Trim();
        var phone = PhoneTextBox.Text.Trim();
        var email = EmailTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
            return;
        }

        var request = new CreateSupplierRequest(name, phone, email);
        var (isSuccess, responseStr) = await ApiService.Instance.CreateSupplierAsync(request);

        if (isSuccess)
        {
            SetStatus(UiConstants.Messages.SUPPLIER_CREATED_SUCCESS, Brushes.Green);
            NameTextBox.Clear();
            PhoneTextBox.Clear();
            EmailTextBox.Clear();
            await LoadDataAsync();
        }
        else
        {
            SetStatus(responseStr, Brushes.Red);
        }
    }

    private async void AddStockLot_Click(object sender, RoutedEventArgs e)
    {
        if (StockSupplierComboBox.SelectedItem is not LookupItem selectedSupplier ||
            StockWarehouseComboBox.SelectedItem is not LookupItem selectedWarehouse ||
            StockRawMaterialComboBox.SelectedItem is not LookupItem selectedRawMaterial)
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
            return;
        }

        var rawQuantityText = StockQuantityTextBox.Text.Replace(',', '.');
        var rawPriceText = StockUnitPriceTextBox.Text.Replace(',', '.');

        if (!decimal.TryParse(rawQuantityText, NumberStyles.Any, CultureInfo.InvariantCulture, out var quantity) ||
            !decimal.TryParse(rawPriceText, NumberStyles.Any, CultureInfo.InvariantCulture, out var unitPrice))
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
            return;
        }

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(Endpoints.STOCK_LOTS, new
        {
            SupplierId = selectedSupplier.Id,
            WarehouseId = selectedWarehouse.Id,
            ItemId = selectedRawMaterial.Id,
            Quantity = quantity,
            UnitPrice = unitPrice,
            BatchNumber = StockBatchTextBox.Text
        });

        if (isSuccess)
        {
            SetStatus($"{UiConstants.Messages.STOCK_LOT_CREATED_SUCCESS} ID: {contentOrError}", Brushes.Green);
            await LoadDataAsync();
        }
        else
        {
            SetStatus(contentOrError, Brushes.Red);
        }
    }

    private void SetStatus(string msg, Brush color)
    {
        StatusTextBlock.Foreground = color;
        StatusTextBlock.Text = LocalizationService.Get(msg);
    }
}