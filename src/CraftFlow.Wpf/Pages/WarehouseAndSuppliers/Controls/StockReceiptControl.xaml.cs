using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Constants.Endpoints;
using CraftFlow.SharedKernel.Dtos.Catalog.Inventory;
using CraftFlow.SharedKernel.Enums.Inventory;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Models.WarehouseAndSuppliers;
using CraftFlow.Wpf.Services;
using CraftFlow.Wpf.Windows;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace CraftFlow.Wpf.Pages.WarehouseAndSuppliers.Controls;

public partial class StockReceiptControl : UserControl
{
    public ObservableCollection<StorageLocationDto> AvailableLocations { get; } = [];
    public ObservableCollection<StorageLocationDto> SelectedLocations { get; } = [];
    public ObservableCollection<ReceiptDraftItem> DraftItems { get; } = [];

    public event Action<string, Brush>? StatusUpdated;
    public event Func<Guid?, Guid?, bool, Task>? FilterChangedRequested;
    public event Func<Task>? StockReceived;

    public StockReceiptControl()
    {
        InitializeComponent();

        AvailableLocationsListBox.ItemsSource = AvailableLocations;
        SelectedLocationsListBox.ItemsSource = SelectedLocations;

        ReceiptDraftDataGrid.ItemsSource = DraftItems;
        DraftItems.CollectionChanged += (s, e) =>
        {
            UpdateDraftSummary();
            ValidateCapacity();
        };
    }

    public void BindSources(
        ObservableCollection<LookupItem> formSuppliers,
        ObservableCollection<LookupItem> filterSuppliers,
        ObservableCollection<LookupItem> formWarehouses,
        ObservableCollection<LookupItem> filterWarehouses,
        ObservableCollection<LookupItem> rawMaterials,
        ObservableCollection<StockLotGridDto> stockLots)
    {
        StockSupplierComboBox.ItemsSource = formSuppliers;
        FilterSupplierComboBox.ItemsSource = filterSuppliers;

        StockWarehouseComboBox.ItemsSource = formWarehouses;
        FilterWarehouseComboBox.ItemsSource = filterWarehouses;

        StockRawMaterialComboBox.ItemsSource = rawMaterials;
        StockLotsDataGrid.ItemsSource = stockLots;
    }

    public void SetDefaultSelections()
    {
        if (StockSupplierComboBox.Items.Count > 0)
            StockSupplierComboBox.SelectedIndex = 0;

        FilterSupplierComboBox.SelectedIndex = 0;
        FilterWarehouseComboBox.SelectedIndex = 0;
    }

    public (Guid? SupplierId, Guid? WarehouseId, bool OnlyExpiringSoon) GetCurrentFilters()
    {
        Guid? supId = FilterSupplierComboBox.SelectedValue is Guid s && s != Guid.Empty ? s : null;
        Guid? whId = FilterWarehouseComboBox.SelectedValue is Guid w && w != Guid.Empty ? w : null;
        bool expiring = FilterExpiringCheckBox.IsChecked ?? false;
        return (supId, whId, expiring);
    }

    private void StockRawMaterialComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (StockRawMaterialComboBox.SelectedItem is LookupItem selectedRaw && !string.IsNullOrWhiteSpace(selectedRaw.Code))
        {
            SelectUnitInComboBox(selectedRaw.Code);
        }
        ValidateCapacity();
    }

    private void SelectUnitInComboBox(string unitTag)
    {
        foreach (ComboBoxItem item in UnitOfMeasureComboBox.Items)
        {
            if (string.Equals(item.Tag?.ToString(), unitTag, StringComparison.OrdinalIgnoreCase))
            {
                UnitOfMeasureComboBox.SelectedItem = item;
                break;
            }
        }
    }

    private string GetSelectedUnitTag() =>
        (UnitOfMeasureComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "l";

    private string GetSelectedUnitDisplayText() =>
        (UnitOfMeasureComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "л";

    private void UnitOfMeasureComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
        ValidateCapacity();

    private async void StockWarehouseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SelectedLocations.Clear();
        DraftItems.Clear();

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
            var locations = await ApiService.Instance.GetAsync<List<StorageLocationDto>>($"{InventoryConstants.WAREHOUSES_LOCATIONS}?warehouseId={warehouseId}");
            AvailableLocations.Clear();

            locations?.Where(l => l.LocationType != StorageLocationType.BREWING_VESSEL
                               && l.Status == StorageLocationStatus.AVAILABLE)
                     .ToList()
                     .ForEach(l => AvailableLocations.Add(l));
        }
        catch { }
    }

    private void AddLocationToSelected_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is StorageLocationDto location)
        {
            AvailableLocations.Remove(location);
            SelectedLocations.Add(location);

            if (!string.IsNullOrWhiteSpace(location.UnitOfMeasure))
            {
                SelectUnitInComboBox(location.UnitOfMeasure);
            }

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

    private void StockQuantityTextBox_TextChanged(object sender, TextChangedEventArgs e) =>
        ValidateCapacity();

    private void ValidateCapacity()
    {
        if (CapacityWarningTextBlock == null || AddDraftLineButton == null || StockQuantityTextBox == null)
            return;

        if (string.IsNullOrWhiteSpace(StockQuantityTextBox.Text))
        {
            CapacityWarningTextBlock.Text = string.Empty;
            AddDraftLineButton.IsEnabled = true;
            return;
        }

        var rawQuantityText = StockQuantityTextBox.Text.Replace(',', '.');
        if (!decimal.TryParse(rawQuantityText, NumberStyles.Any, CultureInfo.InvariantCulture, out var requiredQuantity) || requiredQuantity <= 0)
        {
            CapacityWarningTextBlock.Text = LocalizationService.Get(UiConstants.Messages.INVALID_INPUT_FIELDS);
            CapacityWarningTextBlock.Foreground = Brushes.Red;
            AddDraftLineButton.IsEnabled = false;
            return;
        }

        if (SelectedLocations.Count == 0)
        {
            CapacityWarningTextBlock.Text = string.Empty;
            AddDraftLineButton.IsEnabled = true;
            return;
        }

        var selectedIds = SelectedLocations.Select(l => l.Id).ToHashSet();
        decimal reservedInDraft = DraftItems
            .Where(d => d.StorageLocationIds.Any(id => selectedIds.Contains(id)))
            .Sum(d => d.Quantity);

        decimal totalCapacity = SelectedLocations.Sum(loc =>
        {
            decimal cap = loc.Capacity ?? 0;
            return cap > loc.CurrentVolume ? cap - loc.CurrentVolume : 0;
        });

        decimal effectiveFreeCapacity = Math.Max(0, totalCapacity - reservedInDraft);

        if (effectiveFreeCapacity < requiredQuantity)
        {
            var format = LocalizationService.Get("CAPACITY_WARNING_INSUFFICIENT");
            CapacityWarningTextBlock.Text = string.Format(format, requiredQuantity, effectiveFreeCapacity);
            CapacityWarningTextBlock.Foreground = Brushes.Red;
            AddDraftLineButton.IsEnabled = false;
        }
        else
        {
            var format = LocalizationService.Get("CAPACITY_WARNING_SUFFICIENT");
            CapacityWarningTextBlock.Text = string.Format(format, effectiveFreeCapacity, requiredQuantity);
            CapacityWarningTextBlock.Foreground = Brushes.Green;
            AddDraftLineButton.IsEnabled = true;
        }
    }

    private void DraftInput_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            AddDraftLine_Click(sender, e);
        }
    }

    private void AddDraftLine_Click(object sender, RoutedEventArgs e)
    {
        if (StockRawMaterialComboBox.SelectedItem is not LookupItem selectedRawMaterial)
        {
            StatusUpdated?.Invoke(LocalizationService.Get(UiConstants.Messages.INVALID_INPUT_FIELDS), Brushes.Red);
            return;
        }

        var rawQuantityText = StockQuantityTextBox.Text.Replace(',', '.');
        var rawPriceText = StockUnitPriceTextBox.Text.Replace(',', '.');

        if (!decimal.TryParse(rawQuantityText, NumberStyles.Any, CultureInfo.InvariantCulture, out var quantity) || quantity <= 0 ||
            !decimal.TryParse(rawPriceText, NumberStyles.Any, CultureInfo.InvariantCulture, out var unitPrice) || unitPrice < 0)
        {
            StatusUpdated?.Invoke(LocalizationService.Get(UiConstants.Messages.INVALID_INPUT_FIELDS), Brushes.Red);
            return;
        }

        var batchNumber = string.IsNullOrWhiteSpace(StockBatchTextBox.Text)
            ? InvoiceNumberTextBox.Text.Trim()
            : StockBatchTextBox.Text.Trim();

        var locationIds = SelectedLocations.Select(l => l.Id).ToList();
        var locationsDisplay = SelectedLocations.Count > 0
            ? string.Join(", ", SelectedLocations.Select(l => l.Name))
            : "—";

        DraftItems.Add(new ReceiptDraftItem
        {
            ItemId = selectedRawMaterial.Id,
            ItemName = selectedRawMaterial.Name,
            Quantity = quantity,
            UnitPrice = unitPrice,
            UnitOfMeasure = GetSelectedUnitTag(),
            UnitOfMeasureDisplay = GetSelectedUnitDisplayText(),
            BatchNumber = batchNumber,
            ExpirationDate = ExpirationDatePicker.SelectedDate,
            StorageLocationIds = locationIds,
            StorageLocationsDisplay = locationsDisplay
        });

        foreach (var loc in SelectedLocations.ToList())
        {
            SelectedLocations.Remove(loc);
            AvailableLocations.Add(loc);
        }

        StockRawMaterialComboBox.SelectedItem = null;
        StockRawMaterialComboBox.Text = string.Empty;
        StockQuantityTextBox.Clear();
        StockUnitPriceTextBox.Clear();
        StockBatchTextBox.Clear();
        ExpirationDatePicker.SelectedDate = null;
        CapacityWarningTextBlock.Text = string.Empty;

        StockRawMaterialComboBox.Focus();
    }

    private void RemoveDraftLine_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is ReceiptDraftItem item)
            DraftItems.Remove(item);
    }

    private void ClearDraftLines_Click(object sender, RoutedEventArgs e) =>
        DraftItems.Clear();

    private void ReceiptDraftDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        Dispatcher.BeginInvoke(() =>
        {
            UpdateDraftSummary();
            ValidateCapacity();
        }, DispatcherPriority.Background);
    }

    private void UpdateDraftSummary()
    {
        int count = DraftItems.Count;
        decimal total = DraftItems.Sum(x => x.LineTotal);

        DraftSummaryTextBlock.Text = $"Позиций в накладной: {count} | Итого: ${total:F2}";
        SubmitReceiptBatchButton.IsEnabled = count > 0;
    }

    private void ImportInvoiceFile_Click(object sender, RoutedEventArgs e)
    {
        AlertWindow.ShowSuccess(Window.GetWindow(this), "Модуль автозагрузки накладных из Excel / PDF в разработке. Распознанные позиции будут загружаться в эту таблицу для проверки перед проведением.");
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

    private async void SubmitReceiptBatch_Click(object sender, RoutedEventArgs e)
    {
        if (DraftItems.Count == 0) return;

        if (StockSupplierComboBox.SelectedItem is not LookupItem selectedSupplier ||
            StockWarehouseComboBox.SelectedItem is not LookupItem selectedWarehouse)
        {
            var err = LocalizationService.Get(UiConstants.Messages.INVALID_INPUT_FIELDS);
            StatusUpdated?.Invoke(err, Brushes.Red);
            AlertWindow.ShowError(Window.GetWindow(this), err);
            return;
        }

        var request = new ReceiveStockBatchRequest(
            SupplierId: selectedSupplier.Id,
            WarehouseId: selectedWarehouse.Id,
            InvoiceNumber: string.IsNullOrWhiteSpace(InvoiceNumberTextBox.Text) ? null : InvoiceNumberTextBox.Text.Trim(),
            Items: DraftItems.Select(x => new StockReceiptLineDto(
                ItemId: x.ItemId,
                Quantity: x.Quantity,
                UnitPrice: x.UnitPrice,
                UnitOfMeasureId: Guid.Empty,
                BatchNumber: string.IsNullOrWhiteSpace(x.BatchNumber) ? null : x.BatchNumber,
                ExpirationDate: x.ExpirationDate,
                StorageLocationIds: x.StorageLocationIds.Count > 0 ? x.StorageLocationIds : null
            )).ToList()
        );

        SubmitReceiptBatchButton.IsEnabled = false;
        try
        {
            var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(InventoryConstants.STOCK_LOTS_BATCH, request);

            if (isSuccess)
            {
                DraftItems.Clear();
                var okMsg = LocalizationService.Get(UiConstants.Messages.STOCK_LOT_CREATED_SUCCESS);
                StatusUpdated?.Invoke(okMsg, Brushes.Green);
                AlertWindow.ShowSuccess(Window.GetWindow(this), okMsg);

                if (StockReceived != null)
                    await StockReceived.Invoke();

                if (selectedWarehouse.Id != Guid.Empty)
                    await LoadLocationsForWarehouseAsync(selectedWarehouse.Id);
            }
            else
            {
                StatusUpdated?.Invoke(contentOrError, Brushes.Red);
                AlertWindow.ShowError(Window.GetWindow(this), contentOrError);
            }
        }
        finally
        {
            UpdateDraftSummary();
        }
    }

    private async void FilterStockLots_Changed(object sender, RoutedEventArgs e)
    {
        if (FilterChangedRequested != null)
        {
            var (supId, whId, expiring) = GetCurrentFilters();
            await FilterChangedRequested.Invoke(supId, whId, expiring);
        }
    }

    private async void RefreshStockLots_Click(object sender, RoutedEventArgs e)
    {
        if (FilterChangedRequested != null)
        {
            var (supId, whId, expiring) = GetCurrentFilters();
            await FilterChangedRequested.Invoke(supId, whId, expiring);
        }
    }
}