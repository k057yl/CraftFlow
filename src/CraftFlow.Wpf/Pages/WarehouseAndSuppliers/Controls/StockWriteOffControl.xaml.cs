using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Constants.Endpoints;
using CraftFlow.SharedKernel.Dtos.Aging;
using CraftFlow.SharedKernel.Dtos.Catalog.Inventory;
using CraftFlow.Wpf.Services;
using CraftFlow.Wpf.Windows;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CraftFlow.Wpf.Pages.WarehouseAndSuppliers.Controls;

public partial class StockWriteOffControl : UserControl
{
    private ObservableCollection<StockLotGridDto> _stockLots = [];

    public event Action<string, Brush>? StatusUpdated;
    public event Func<Task>? WriteOffCompleted;

    public StockWriteOffControl()
    {
        InitializeComponent();
    }

    public void BindSources(ObservableCollection<StockLotGridDto> stockLots)
    {
        _stockLots = stockLots;
        WriteOffDataGrid.ItemsSource = _stockLots;
    }

    private async void WriteOffRow_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not Guid lotId) return;

        var lot = _stockLots.FirstOrDefault(l => l.Id == lotId);
        if (lot == null) return;

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

            if (WriteOffCompleted != null)
                await WriteOffCompleted.Invoke();
        }
        else
        {
            StatusUpdated?.Invoke(contentOrError, Brushes.Red);
            AlertWindow.ShowError(Window.GetWindow(this), contentOrError);
        }
    }
}