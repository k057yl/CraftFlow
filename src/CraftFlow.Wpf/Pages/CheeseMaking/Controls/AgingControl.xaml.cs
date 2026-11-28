using CraftFlow.SharedKernel.Constants.Endpoints;
using CraftFlow.SharedKernel.Dtos.Aging;
using CraftFlow.SharedKernel.Dtos.Catalog.Inventory;
using CraftFlow.SharedKernel.Dtos.Production;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static CraftFlow.Wpf.Services.LocalizationService;

namespace CraftFlow.Wpf.Pages.CheeseMaking.Controls;

public partial class AgingControl : UserControl
{
    private const int MaxBatchNameLength = 85;

    private ObservableCollection<LookupItem> _warehouses = [];
    public ObservableCollection<LookupItem> Warehouses
    {
        get => _warehouses;
        set { _warehouses = value; TargetWarehousesComboBox.ItemsSource = _warehouses; }
    }

    private ObservableCollection<BatchReadyForAgingDto> _completedBatches = [];
    public ObservableCollection<BatchReadyForAgingDto> CompletedBatches
    {
        get => _completedBatches;
        set { _completedBatches = value; CompletedBatchesComboBox.ItemsSource = _completedBatches; }
    }

    private ObservableCollection<LookupItem> _agingChambers = [];
    public ObservableCollection<LookupItem> AgingChambers
    {
        get => _agingChambers;
        set { _agingChambers = value; AgingChambersComboBox.ItemsSource = _agingChambers; }
    }

    private ObservableCollection<LookupItem> _activeAgingLots = [];
    public ObservableCollection<LookupItem> ActiveAgingLots
    {
        get => _activeAgingLots;
        set { _activeAgingLots = value; ActiveAgingLotsComboBox.ItemsSource = _activeAgingLots; }
    }

    private ObservableCollection<AgingLotSummaryDto> _agingLotsSummary = [];
    public ObservableCollection<AgingLotSummaryDto> AgingLotsSummary
    {
        get => _agingLotsSummary;
        set { _agingLotsSummary = value; AgingLotsDataGrid.ItemsSource = _agingLotsSummary; }
    }

    public ObservableCollection<StorageLocationDto> TargetStorageLocations { get; set; } = [];

    public TextBlock? StatusTextBlock { get; set; }
    public CheeseMakingPage? ParentPage { get; set; }

    public AgingControl()
    {
        InitializeComponent();

        CompletedBatchesComboBox.ItemsSource = CompletedBatches;
        AgingChambersComboBox.ItemsSource = AgingChambers;
        AgingLotsDataGrid.ItemsSource = AgingLotsSummary;
        ActiveAgingLotsComboBox.ItemsSource = ActiveAgingLots;
        TargetWarehousesComboBox.ItemsSource = Warehouses;
        TargetStorageLocationsListBox.ItemsSource = TargetStorageLocations;
    }

    private void CompletedBatchesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CompletedBatchesComboBox.SelectedItem is BatchReadyForAgingDto selectedBatch)
        {
            MinAgingDaysTextBox.Text = selectedBatch.DefaultAgingDays.ToString();
            AgingLotNameTextBox.Text = selectedBatch.Name;
        }
        else
        {
            MinAgingDaysTextBox.Clear();
            AgingLotNameTextBox.Clear();
        }
    }

    private async void TransferToAging_Click(object sender, RoutedEventArgs e)
    {
        if (CompletedBatchesComboBox.SelectedItem is not BatchReadyForAgingDto selectedBatch ||
            AgingChambersComboBox.SelectedValue is not Guid chamberId ||
            !int.TryParse(MinAgingDaysTextBox.Text.Trim(), out var minDays))
        {
            SetStatus("UI_ERR_INVALID_INPUT_FIELDS", Brushes.Red);
            return;
        }

        var customLotName = AgingLotNameTextBox.Text?.Trim();
        if (!string.IsNullOrEmpty(customLotName) && customLotName.Length > MaxBatchNameLength)
        {
            SetStatusRaw($"Максимальная длина маркировки — {MaxBatchNameLength} символов.", Brushes.Red);
            return;
        }

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(AgingConstants.AGING_LOTS_TRANSFER, new
        {
            ProductionBatchId = selectedBatch.Id,
            AgingChamberId = chamberId,
            MinAgingDays = minDays,
            UnitsCount = selectedBatch.UnitsCount,
            CustomBatchNumber = string.IsNullOrWhiteSpace(customLotName) ? null : customLotName,
            StorageLocationId = (Guid?)null
        });

        if (isSuccess)
        {
            SetStatus("UI_AGING_MSG_TRANSFER_SUCCESS", Brushes.Green);

            CompletedBatchesComboBox.SelectedValue = null;
            AgingChambersComboBox.SelectedValue = null;
            AgingLotNameTextBox.Clear();

            if (ParentPage != null)
            {
                await ParentPage.LoadDataExternalAsync();
            }
        }
        else
        {
            SetStatusRaw(contentOrError, Brushes.Red);
        }
    }

    private void AgingLotsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (AgingLotsDataGrid.SelectedItem is AgingLotSummaryDto selectedLot)
        {
            ActiveAgingLotsComboBox.SelectedValue = selectedLot.LotId;
        }
    }

    private void ActiveAgingLotsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ActiveAgingLotsComboBox.SelectedItem is LookupItem selectedLot)
        {
            ReleaseLotNameTextBox.Text = selectedLot.Name;
        }
        else
        {
            ReleaseLotNameTextBox.Clear();
        }
    }

    private async void TargetWarehousesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        TargetStorageLocations.Clear();
        if (TargetWarehousesComboBox.SelectedValue is Guid warehouseId && warehouseId != Guid.Empty)
        {
            try
            {
                var locations = await ApiService.Instance.GetAsync<List<StorageLocationDto>>($"{InventoryConstants.WAREHOUSES}/locations?warehouseId={warehouseId}");
                locations?.ForEach(TargetStorageLocations.Add);
            }
            catch { }
        }
    }

    private async void ReleaseFromAging_Click(object sender, RoutedEventArgs e)
    {
        if (ActiveAgingLotsComboBox.SelectedValue is not Guid lotId)
        {
            SetStatus("UI_AGING_ERR_SELECT_LOT", Brushes.Red);
            return;
        }

        if (TargetWarehousesComboBox.SelectedValue is not Guid warehouseId)
        {
            SetStatus("UI_AGING_ERR_SELECT_WAREHOUSE", Brushes.Red);
            return;
        }

        var customLotName = ReleaseLotNameTextBox.Text?.Trim();
        if (!string.IsNullOrEmpty(customLotName) && customLotName.Length > MaxBatchNameLength)
        {
            SetStatusRaw($"Максимальная длина маркировки — {MaxBatchNameLength} символов.", Brushes.Red);
            return;
        }

        var details = await ApiService.Instance.GetAsync<GetAgingLotDetailsDto>($"{AgingConstants.AGING_LOTS_ACTIVE}/{lotId}");
        if (details == null) return;

        var items = await ApiService.Instance.GetAsync<List<GetAgingLotItemDto>>($"{AgingConstants.AGING_LOTS_ACTIVE}/{lotId}/items");

        var selectedLotName = (ActiveAgingLotsComboBox.SelectedItem as LookupItem)?.Name ?? string.Empty;
        var selectedChamberName = AgingLotsSummary.FirstOrDefault(l => l.LotId == lotId)?.ChamberName ?? string.Empty;
        var weighWindow = new LotReleaseWeighingWindow(
            selectedLotName,
            selectedChamberName,
            details.TotalBatchCost,
            details.UnitsCount,
            items
        )
        {
            Owner = Window.GetWindow(this)
        };

        if (weighWindow.ShowDialog() == true)
        {
            var selectedStorageLocationIds = TargetStorageLocationsListBox.SelectedItems
                .OfType<StorageLocationDto>()
                .Select(l => l.Id)
                .ToList();

            var response = await ApiService.Instance.PostAndReadAsync(AgingConstants.AGING_LOTS_RELEASE, new
            {
                AgingLotId = lotId,
                TargetWarehouseId = warehouseId,
                ActualFinalQuantity = weighWindow.TotalWeight,
                UnitsCount = weighWindow.ValidUnitsCount,
                UnitPrice = weighWindow.UnitPrice,
                CustomBatchNumber = string.IsNullOrWhiteSpace(customLotName) ? null : customLotName,
                StorageLocationIds = selectedStorageLocationIds.Count > 0 ? selectedStorageLocationIds : null,
                HeadWeights = weighWindow.ValidHeadWeights,
                DiscardedItemIds = weighWindow.DiscardedItemIds.Count > 0 ? weighWindow.DiscardedItemIds : null,
                ExpirationDate = weighWindow.ExpirationDate
            });

            if (response.IsSuccess)
            {
                SetStatus("UI_AGING_MSG_RELEASE_SUCCESS", Brushes.Green);
                ReleaseLotNameTextBox.Clear();
                TargetStorageLocations.Clear();

                if (ParentPage != null)
                {
                    await ParentPage.LoadDataExternalAsync();
                }
            }
            else
            {
                SetStatusFormatted("UI_ERR_DATA_LOAD_FMT", Brushes.Red, response.ContentOrError);
            }
        }
    }

    private void SetStatus(string resourceKey, Brush color)
    {
        if (StatusTextBlock == null) return;
        StatusTextBlock.Foreground = color;
        StatusTextBlock.Text = Get(resourceKey);
    }

    private void SetStatusFormatted(string resourceKey, Brush color, params object[] args)
    {
        if (StatusTextBlock == null) return;
        StatusTextBlock.Foreground = color;
        var format = Get(resourceKey);
        StatusTextBlock.Text = string.Format(format, args);
    }

    private void SetStatusRaw(string rawText, Brush color)
    {
        if (StatusTextBlock == null) return;
        StatusTextBlock.Foreground = color;
        StatusTextBlock.Text = rawText;
    }
}