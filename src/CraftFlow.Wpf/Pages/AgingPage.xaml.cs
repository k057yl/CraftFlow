using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Services;

namespace CraftFlow.Wpf.Pages;

public partial class AgingPage : Page
{
    public ObservableCollection<LookupItem> CompletedBatches { get; } = [];
    public ObservableCollection<LookupItem> AgingChambers { get; } = [];
    public ObservableCollection<LookupItem> ActiveAgingLots { get; } = [];
    public ObservableCollection<LookupItem> TargetWarehouses { get; } = [];

    public AgingPage()
    {
        InitializeComponent();

        CompletedBatchesComboBox.ItemsSource = CompletedBatches;
        AgingChambersComboBox.ItemsSource = AgingChambers;
        ActiveAgingLotsComboBox.ItemsSource = ActiveAgingLots;
        TargetWarehousesComboBox.ItemsSource = TargetWarehouses;

        Loaded += async (s, e) => await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var batches = await ApiService.Instance.GetAsync<List<LookupDto>>(Endpoints.BATCHES_ACTIVE);
            CompletedBatches.Clear();
            batches?.ForEach(b => CompletedBatches.Add(new LookupItem(b.Id, b.Name)));

            var warehouses = await ApiService.Instance.GetAsync<List<LookupDto>>(Endpoints.WAREHOUSES);
            AgingChambers.Clear();
            warehouses?.ForEach(c => AgingChambers.Add(new LookupItem(c.Id, c.Name)));

            var lots = await ApiService.Instance.GetAsync<List<LookupDto>>(Endpoints.AGING_LOTS_ACTIVE);
            ActiveAgingLots.Clear();
            lots?.ForEach(l => ActiveAgingLots.Add(new LookupItem(l.Id, l.Name)));

            TargetWarehouses.Clear();
            warehouses?.ForEach(w => TargetWarehouses.Add(new LookupItem(w.Id, w.Name)));

            SetStatus(UiConstants.Messages.DATA_LOADED_SUCCESS, Brushes.Green);
        }
        catch (Exception ex)
        {
            SetStatus($"{UiConstants.Messages.DATA_LOAD_ERROR}: {ex.Message}", Brushes.Red);
        }
    }

    private async void TransferToAging_Click(object sender, RoutedEventArgs e)
    {
        if (CompletedBatchesComboBox.SelectedValue is not Guid batchId ||
            AgingChambersComboBox.SelectedValue is not Guid chamberId ||
            !int.TryParse(MinAgingDaysTextBox.Text.Trim(), out var minDays))
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
            return;
        }

        var (isSuccess, contentOrError) = await ApiService.Instance.TransferToAgingAsync(new TransferToAgingRequest(batchId, chamberId, minDays));

        if (isSuccess)
        {
            SetStatus(UiConstants.Messages.LOT_TRANSFERRED_TO_AGING_SUCCESS, Brushes.Green);
            await LoadDataAsync();
        }
        else
        {
            SetStatus(contentOrError, Brushes.Red);
        }
    }

    private async void ReleaseFromAging_Click(object sender, RoutedEventArgs e)
    {
        if (ActiveAgingLotsComboBox.SelectedValue is not Guid lotId ||
            TargetWarehousesComboBox.SelectedValue is not Guid warehouseId ||
            !decimal.TryParse(ActualFinalQuantityTextBox.Text.Trim(), out var finalQty) ||
            !decimal.TryParse(UnitPriceTextBox.Text.Trim(), out var unitPrice))
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
            return;
        }

        var (isSuccess, contentOrError) = await ApiService.Instance.ReleaseFromAgingAsync(new ReleaseFromAgingRequest(lotId, warehouseId, finalQty, unitPrice));

        if (isSuccess)
        {
            SetStatus(UiConstants.Messages.LOT_RELEASED_FROM_AGING_SUCCESS, Brushes.Green);
            ActualFinalQuantityTextBox.Clear();
            UnitPriceTextBox.Clear();
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