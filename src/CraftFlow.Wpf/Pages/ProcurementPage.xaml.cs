using CraftFlow.Api.Modules.Catalog;
using CraftFlow.Api.Modules.Inventory;
using CraftFlow.Api.Modules.Procurement;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Dtos.Aging;
using CraftFlow.SharedKernel.Dtos.Common;
using CraftFlow.SharedKernel.Dtos.Inventory;
using CraftFlow.SharedKernel.Dtos.Supplier;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Services;
using CraftFlow.Wpf.Windows;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CraftFlow.Wpf.Pages;

public partial class ProcurementPage : Page
{
    public ObservableCollection<LookupItem> FormSuppliers { get; } = [];
    public ObservableCollection<LookupItem> FilterSuppliers { get; } = [];

    public ObservableCollection<LookupItem> FormWarehouses { get; } = [];
    public ObservableCollection<LookupItem> FilterWarehouses { get; } = [];

    public ObservableCollection<LookupItem> RawMaterials { get; } = [];
    public ObservableCollection<StockLotGridDto> StockLots { get; } = [];

    public ObservableCollection<StorageLocationDto> AvailableLocations { get; } = [];
    public ObservableCollection<StorageLocationDto> SelectedLocations { get; } = [];

    private bool _isDataLoaded = false;

    public ProcurementPage()
    {
        InitializeComponent();

        StockSupplierComboBox.ItemsSource = FormSuppliers;
        FilterSupplierComboBox.ItemsSource = FilterSuppliers;

        StockWarehouseComboBox.ItemsSource = FormWarehouses;
        FilterWarehouseComboBox.ItemsSource = FilterWarehouses;

        StockRawMaterialComboBox.ItemsSource = RawMaterials;
        StockLotsDataGrid.ItemsSource = StockLots;
        WriteOffDataGrid.ItemsSource = StockLots;

        AvailableLocationsListBox.ItemsSource = AvailableLocations;
        SelectedLocationsListBox.ItemsSource = SelectedLocations;

        Loaded += async (s, e) => await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            _isDataLoaded = false;

            var suppliers = await ApiService.Instance.GetAsync<List<LookupDto>>(ProcurementConstants.SUPPLIERS) ?? [];
            FormSuppliers.Clear();
            FilterSuppliers.Clear();
            FilterSuppliers.Add(new LookupItem(Guid.Empty, LocalizationService.Get(UiConstants.Filters.ALL_SUPPLIERS)));
            suppliers.ForEach(s =>
            {
                var item = new LookupItem(s.Id, s.Name);
                FormSuppliers.Add(item);
                FilterSuppliers.Add(item);
            });
            FilterSupplierComboBox.SelectedIndex = 0;

            var warehouses = await ApiService.Instance.GetAsync<List<LookupDto>>(InventoryConstants.WAREHOUSES) ?? [];
            FormWarehouses.Clear();
            FilterWarehouses.Clear();
            FilterWarehouses.Add(new LookupItem(Guid.Empty, LocalizationService.Get(UiConstants.Filters.ALL_WAREHOUSES)));
            warehouses.ForEach(w =>
            {
                var item = new LookupItem(w.Id, w.Name);
                FormWarehouses.Add(item);
                FilterWarehouses.Add(item);
            });
            FilterWarehouseComboBox.SelectedIndex = 0;

            var raw = await ApiService.Instance.GetAsync<List<LookupDto>>(CatalogConstants.RAW_MATERIALS) ?? [];
            RawMaterials.Clear();
            raw.ForEach(r => RawMaterials.Add(new LookupItem(r.Id, r.Name)));

            _isDataLoaded = true;
            await LoadStockLotsAsync();

            SetStatus(UiConstants.Messages.DATA_LOADED_SUCCESS, Brushes.Green);
        }
        catch (Exception ex)
        {
            SetStatusFormatted(UiConstants.Messages.DATA_LOAD_ERROR, Brushes.Red, ex.Message);
            AlertWindow.ShowError(Window.GetWindow(this), ex.Message);
        }
    }

    private async void StockWarehouseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SelectedLocations.Clear();
        if (StockWarehouseComboBox.SelectedValue is Guid warehouseId && warehouseId != Guid.Empty)
        {
            await LoadLocationsForWarehouseAsync(warehouseId);
        }
        else
        {
            AvailableLocations.Clear();
        }
        ValidateCapacity();
    }

    private async Task LoadLocationsForWarehouseAsync(Guid warehouseId)
    {
        try
        {
            var locations = await ApiService.Instance.GetAsync<List<StorageLocationDto>>($"{InventoryConstants.WAREHOUSES}/locations?warehouseId={warehouseId}");
            AvailableLocations.Clear();
            locations?.ForEach(l => AvailableLocations.Add(l));
        }
        catch { }
    }

    private void AddLocationToSelected_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is StorageLocationDto location)
        {
            AvailableLocations.Remove(location);
            SelectedLocations.Add(location);
            ValidateCapacity();
        }
    }

    private void RemoveLocationFromSelected_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is StorageLocationDto location)
        {
            SelectedLocations.Remove(location);
            AvailableLocations.Add(location);
            ValidateCapacity();
        }
    }

    private void StockQuantityTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ValidateCapacity();
    }

    private void ValidateCapacity()
    {
        if (CapacityWarningTextBlock == null || AddStockLotButton == null || StockQuantityTextBox == null)
        {
            return;
        }

        var rawQuantityText = StockQuantityTextBox.Text.Replace(',', '.');
        if (!decimal.TryParse(rawQuantityText, NumberStyles.Any, CultureInfo.InvariantCulture, out var requiredQuantity) || requiredQuantity <= 0)
        {
            CapacityWarningTextBlock.Text = LocalizationService.Get(UiConstants.Messages.INVALID_INPUT_FIELDS);
            CapacityWarningTextBlock.Foreground = Brushes.Red;
            AddStockLotButton.IsEnabled = false;
            return;
        }

        if (SelectedLocations.Count == 0)
        {
            CapacityWarningTextBlock.Text = string.Empty;
            AddStockLotButton.IsEnabled = true;
            return;
        }

        decimal totalCapacity = 0;
        foreach (var loc in SelectedLocations)
        {
            decimal cap = loc.Capacity ?? 0;
            decimal freeCap = cap > loc.CurrentVolume ? cap - loc.CurrentVolume : 0;
            totalCapacity += freeCap;
        }

        if (totalCapacity < requiredQuantity)
        {
            var template = LocalizationService.Get(UiConstants.Capacity.WARNING_INSUFFICIENT);
            CapacityWarningTextBlock.Text = string.Format(template, requiredQuantity, totalCapacity);
            CapacityWarningTextBlock.Foreground = Brushes.Red;
            AddStockLotButton.IsEnabled = false;
        }
        else
        {
            var template = LocalizationService.Get(UiConstants.Capacity.WARNING_SUFFICIENT);
            CapacityWarningTextBlock.Text = string.Format(template, totalCapacity, requiredQuantity);
            CapacityWarningTextBlock.Foreground = Brushes.Green;
            AddStockLotButton.IsEnabled = true;
        }
    }

    private async Task LoadStockLotsAsync()
    {
        if (!_isDataLoaded) return;

        try
        {
            Guid? selectedSupplierId = FilterSupplierComboBox.SelectedValue is Guid supId && supId != Guid.Empty ? supId : null;
            Guid? selectedWarehouseId = FilterWarehouseComboBox.SelectedValue is Guid whId && whId != Guid.Empty ? whId : null;
            bool onlyExpiringSoon = FilterExpiringCheckBox.IsChecked ?? false;

            var queryParams = new List<string> { $"onlyExpiringSoon={onlyExpiringSoon}" };
            if (selectedSupplierId.HasValue) queryParams.Add($"supplierId={selectedSupplierId.Value}");
            if (selectedWarehouseId.HasValue) queryParams.Add($"warehouseId={selectedWarehouseId.Value}");

            string queryUrl = $"{InventoryConstants.STOCK_LOTS}?{string.Join("&", queryParams)}";
            var lots = await ApiService.Instance.GetAsync<List<StockLotGridDto>>(queryUrl);

            StockLots.Clear();

            if (lots != null)
            {
                foreach (var lot in lots.Where(l => l.Quantity > 0m))
                {
                    StockLots.Add(lot);
                }
            }
        }
        catch { }
    }

    private async void FilterStockLots_Changed(object sender, RoutedEventArgs e) => await LoadStockLotsAsync();
    private async void RefreshStockLots_Click(object sender, RoutedEventArgs e) => await LoadStockLotsAsync();

    private async void CreateSupplier_Click(object sender, RoutedEventArgs e)
    {
        var name = NameTextBox.Text.Trim();
        var phone = string.IsNullOrWhiteSpace(PhoneTextBox.Text) ? null : PhoneTextBox.Text.Trim();
        var email = string.IsNullOrWhiteSpace(EmailTextBox.Text) ? null : EmailTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
            AlertWindow.ShowError(Window.GetWindow(this), UiConstants.Messages.INVALID_INPUT_FIELDS);
            return;
        }

        var request = new CreateSupplierRequest(name, phone, email);
        var (isSuccess, responseStr) = await ApiService.Instance.CreateSupplierAsync(request);

        if (isSuccess)
        {
            SetStatus(UiConstants.Messages.SUPPLIER_CREATED_SUCCESS, Brushes.Green);
            AlertWindow.ShowSuccess(Window.GetWindow(this), UiConstants.Messages.SUPPLIER_CREATED_SUCCESS);
            NameTextBox.Clear();
            PhoneTextBox.Clear();
            EmailTextBox.Clear();
            await LoadDataAsync();
        }
        else
        {
            SetStatusRaw(responseStr, Brushes.Red);
            AlertWindow.ShowError(Window.GetWindow(this), responseStr);
        }
    }

    private async void AddStockLot_Click(object sender, RoutedEventArgs e)
    {
        if (StockSupplierComboBox.SelectedItem is not LookupItem selectedSupplier ||
            StockWarehouseComboBox.SelectedItem is not LookupItem selectedWarehouse ||
            StockRawMaterialComboBox.SelectedItem is not LookupItem selectedRawMaterial)
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
            AlertWindow.ShowError(Window.GetWindow(this), UiConstants.Messages.INVALID_INPUT_FIELDS);
            return;
        }

        var rawQuantityText = StockQuantityTextBox.Text.Replace(',', '.');
        var rawPriceText = StockUnitPriceTextBox.Text.Replace(',', '.');

        if (!decimal.TryParse(rawQuantityText, NumberStyles.Any, CultureInfo.InvariantCulture, out var quantity) ||
            !decimal.TryParse(rawPriceText, NumberStyles.Any, CultureInfo.InvariantCulture, out var unitPrice))
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
            AlertWindow.ShowError(Window.GetWindow(this), UiConstants.Messages.INVALID_INPUT_FIELDS);
            return;
        }

        int? unitsCount = null;
        if (int.TryParse(StockUnitsCountTextBox.Text.Trim(), out var parsedUnits) && parsedUnits > 0)
        {
            unitsCount = parsedUnits;
        }

        DateTime? expirationDate = ExpirationDatePicker.SelectedDate;

        var selectedLocationIds = SelectedLocations.Select(l => l.Id).ToList();

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(InventoryConstants.STOCK_LOTS, new
        {
            SupplierId = selectedSupplier.Id,
            WarehouseId = selectedWarehouse.Id,
            ItemId = selectedRawMaterial.Id,
            Quantity = quantity,
            UnitsCount = unitsCount,
            UnitPrice = unitPrice,
            BatchNumber = StockBatchTextBox.Text,
            ExpirationDate = expirationDate,
            StorageLocationIds = selectedLocationIds.Count > 0 ? selectedLocationIds : null
        });

        if (isSuccess)
        {
            SetStatus(UiConstants.Messages.STOCK_LOT_CREATED_SUCCESS, Brushes.Green);
            AlertWindow.ShowSuccess(Window.GetWindow(this), UiConstants.Messages.STOCK_LOT_CREATED_SUCCESS);
            StockUnitsCountTextBox.Clear();
            ExpirationDatePicker.SelectedDate = null;
            SelectedLocations.Clear();
            await LoadStockLotsAsync();
            if (selectedWarehouse.Id != Guid.Empty)
            {
                await LoadLocationsForWarehouseAsync(selectedWarehouse.Id);
            }
        }
        else
        {
            SetStatusRaw(contentOrError, Brushes.Red);
            AlertWindow.ShowError(Window.GetWindow(this), contentOrError);
        }
    }

    private async void WriteOffRow_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Guid lotId)
        {
            var lot = StockLots.FirstOrDefault(l => l.Id == lotId);
            if (lot != null)
            {
                if (lot.Quantity <= 0m)
                {
                    SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
                    AlertWindow.ShowError(Window.GetWindow(this), UiConstants.Messages.INVALID_INPUT_FIELDS);
                    return;
                }

                List<GetAgingLotItemDto>? agingItems = await ApiService.Instance.GetAsync<List<GetAgingLotItemDto>>($"{InventoryConstants.STOCK_LOTS}/{lot.Id}/items");

                var dialog = new WriteOffDialog(lot.ItemName, lot.BatchNumber, lot.Quantity, items: agingItems)
                {
                    Owner = Window.GetWindow(this)
                };

                if (dialog.ShowDialog() == true)
                {
                    await ExecuteWriteOffAsync(lot.Id, dialog.QuantityToWriteOff, dialog.SelectedItemIds, dialog.Reason);
                }
            }
        }
    }

    private async void QuickCreateLocation_Click(object sender, RoutedEventArgs e)
    {
        Guid? currentWarehouseId = StockWarehouseComboBox.SelectedValue as Guid?;

        var dialog = new CreateStorageLocationWindow(currentWarehouseId)
        {
            Owner = Window.GetWindow(this)
        };

        if (dialog.ShowDialog() == true && currentWarehouseId.HasValue && currentWarehouseId != Guid.Empty)
        {
            await LoadLocationsForWarehouseAsync(currentWarehouseId.Value);
        }
    }

    private async Task ExecuteWriteOffAsync(Guid stockLotId, decimal quantity, List<Guid>? itemIds = null, string? reason = null)
    {
        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync($"{InventoryConstants.STOCK_LOTS}/write-off", new
        {
            StockLotId = stockLotId,
            QuantityToWriteOff = quantity,
            SelectedItemIds = itemIds,
            Reason = reason
        });

        if (isSuccess)
        {
            SetStatus(UiConstants.Messages.DATA_LOADED_SUCCESS, Brushes.Green);
            AlertWindow.ShowSuccess(Window.GetWindow(this), UiConstants.Messages.DATA_LOADED_SUCCESS);
            await LoadStockLotsAsync();
        }
        else
        {
            SetStatusRaw(contentOrError, Brushes.Red);
            AlertWindow.ShowError(Window.GetWindow(this), contentOrError);
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