using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Constants.Endpoints;
using CraftFlow.SharedKernel.Dtos.Aging;
using CraftFlow.SharedKernel.Dtos.Catalog.Inventory;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Services;
using CraftFlow.Wpf.Windows;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CraftFlow.Wpf.Pages.WarehouseAndSuppliers.Controls;

public partial class StockReceiptControl : UserControl
{
    private ObservableCollection<LookupItem> _formSuppliers = [];
    private ObservableCollection<LookupItem> _formWarehouses = [];
    private ObservableCollection<LookupItem> _rawMaterials = [];
    private ObservableCollection<StockLotGridDto> _stockLots = [];

    public event Action<string, Brush>? StatusUpdated;
    public event Func<Guid?, Guid?, bool, Task>? FilterChangedRequested;
    public event Func<Task>? StockChanged;

    public StockReceiptControl()
    {
        InitializeComponent();
    }

    public void BindSources(
        ObservableCollection<LookupItem> formSuppliers,
        ObservableCollection<LookupItem> filterSuppliers,
        ObservableCollection<LookupItem> formWarehouses,
        ObservableCollection<LookupItem> filterWarehouses,
        ObservableCollection<LookupItem> rawMaterials,
        ObservableCollection<StockLotGridDto> stockLots)
    {
        _formSuppliers = formSuppliers;
        _formWarehouses = formWarehouses;
        _rawMaterials = rawMaterials;
        _stockLots = stockLots;

        FilterSupplierComboBox.ItemsSource = filterSuppliers;
        FilterWarehouseComboBox.ItemsSource = filterWarehouses;
        StockLotsDataGrid.ItemsSource = _stockLots;
    }

    public void SetDefaultSelections()
    {
        if (FilterSupplierComboBox.Items.Count > 0)
            FilterSupplierComboBox.SelectedIndex = 0;

        if (FilterWarehouseComboBox.Items.Count > 0)
            FilterWarehouseComboBox.SelectedIndex = 0;
    }

    public (Guid? SupplierId, Guid? WarehouseId, bool OnlyExpiringSoon) GetCurrentFilters()
    {
        Guid? supId = FilterSupplierComboBox.SelectedValue is Guid s && s != Guid.Empty ? s : null;
        Guid? whId = FilterWarehouseComboBox.SelectedValue is Guid w && w != Guid.Empty ? w : null;
        bool expiring = FilterExpiringCheckBox.IsChecked ?? false;
        return (supId, whId, expiring);
    }

    private async void OpenReceiveStockModal_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ReceiveStockBatchWindow(_formSuppliers, _formWarehouses, _rawMaterials)
        {
            Owner = Window.GetWindow(this)
        };

        if (dialog.ShowDialog() == true)
        {
            var okMsg = LocalizationService.Get(UiConstants.Messages.STOCK_LOT_CREATED_SUCCESS);
            StatusUpdated?.Invoke(okMsg, Brushes.Green);
            AlertWindow.ShowSuccess(Window.GetWindow(this), okMsg);

            if (StockChanged != null)
                await StockChanged.Invoke();
        }
    }

    private void OpenStockLotMenu_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not StockLotGridDto lot) return;

        var menu = new ContextMenu();
        var writeOffItem = new MenuItem { Header = LocalizationService.Get("BTN_WRITE_OFF") };

        writeOffItem.Click += async (s, args) => await OpenWriteOffDialogAsync(lot);

        menu.Items.Add(writeOffItem);
        menu.PlacementTarget = btn;
        menu.IsOpen = true;
    }

    private async Task OpenWriteOffDialogAsync(StockLotGridDto lot)
    {
        if (lot.Quantity <= 0m)
        {
            var err = LocalizationService.Get(UiConstants.Messages.INVALID_INPUT_FIELDS);
            StatusUpdated?.Invoke(err, Brushes.Red);
            AlertWindow.ShowError(Window.GetWindow(this), err);
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

    private async Task ExecuteWriteOffAsync(Guid stockLotId, decimal quantity, List<Guid>? itemIds = null, string? reason = null)
    {
        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(InventoryConstants.STOCK_LOTS_WRITE_OFF, new
        {
            StockLotId = stockLotId,
            QuantityToWriteOff = quantity,
            SelectedItemIds = itemIds,
            Reason = reason
        });

        if (isSuccess)
        {
            var msg = LocalizationService.Get(UiConstants.Messages.DATA_LOADED_SUCCESS);
            StatusUpdated?.Invoke(msg, Brushes.Green);
            AlertWindow.ShowSuccess(Window.GetWindow(this), msg);

            if (StockChanged != null)
                await StockChanged.Invoke();
        }
        else
        {
            StatusUpdated?.Invoke(contentOrError, Brushes.Red);
            AlertWindow.ShowError(Window.GetWindow(this), contentOrError);
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