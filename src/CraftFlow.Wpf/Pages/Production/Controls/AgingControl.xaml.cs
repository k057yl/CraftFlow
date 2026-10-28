using CraftFlow.Api.Modules.Aging;
using CraftFlow.Api.Modules.Inventory;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Dtos.Aging;
using CraftFlow.SharedKernel.Dtos.Inventory;
using CraftFlow.SharedKernel.Dtos.Production;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Pages.Production.Windows;
using CraftFlow.Wpf.Services;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static CraftFlow.Wpf.Services.LocalizationService;

namespace CraftFlow.Wpf.Pages.Production.Controls;

public partial class AgingControl : UserControl
{
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

    private decimal _selectedLotTotalCost;
    private string _selectedLotUnitName = FormattingConstants.DEFAULT_WEIGHT_UNIT;

    public TextBlock? StatusTextBlock { get; set; }
    public ProductionPage? ParentPage { get; set; }

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

    private void CompletedBatchesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CompletedBatchesComboBox.SelectedItem is BatchReadyForAgingDto selectedBatch)
        {
            MinAgingDaysTextBox.Text = selectedBatch.DefaultAgingDays.ToString();
            AgingLotNameTextBox.Text = string.Concat(selectedBatch.Name, " (", Get("UI_TAB_AGING"), ")");
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

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(AgingConstants.AGING_LOTS_TRANSFER, new TransferToAgingRequest(
            selectedBatch.Id,
            chamberId,
            minDays,
            selectedBatch.UnitsCount,
            string.IsNullOrWhiteSpace(customLotName) ? null : customLotName
        ));

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

    private async void ActiveAgingLotsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ActiveAgingLotsComboBox.SelectedValue is not Guid lotId || lotId == Guid.Empty)
        {
            ReleaseLotNameTextBox.Clear();
            ActualFinalQuantityTextBox.Clear();
            ReleaseUnitsCountTextBox.Text = "1";
            UnitPriceTextBox.Clear();
            CalculatedUnitCostTextBlock.Text = "$ 0.00";
            _selectedLotTotalCost = 0;
            return;
        }

        if (ActiveAgingLotsComboBox.SelectedItem is LookupItem selectedLot)
        {
            ReleaseLotNameTextBox.Text = string.Concat(selectedLot.Name, " (", Get("UI_AGING_GROUP_RELEASE"), ")");

            try
            {
                var details = await ApiService.Instance.GetAsync<GetAgingLotDetailsDto>($"{AgingConstants.AGING_LOTS_ACTIVE}/{lotId}");
                if (details != null)
                {
                    _selectedLotTotalCost = details.TotalBatchCost;
                    _selectedLotUnitName = string.IsNullOrWhiteSpace(details.UnitName) ? FormattingConstants.DEFAULT_WEIGHT_UNIT : details.UnitName;

                    var costFormat = Get("UI_AGING_LABEL_CALCULATED_UNIT_COST_FMT");
                    CalculatedCostLabelTextBlock.Text = string.Format(costFormat, _selectedLotUnitName);

                    var priceFormat = Get("UI_AGING_LABEL_SELLING_PRICE_PER_UNIT_FMT");
                    UnitPriceLabelTextBlock.Text = string.Format(priceFormat, _selectedLotUnitName);

                    ActualFinalQuantityTextBox.TextChanged -= ActualFinalQuantityTextBox_TextChanged;

                    ActualFinalQuantityTextBox.Text = details.InitialQuantity.ToString("F2", CultureInfo.InvariantCulture);
                    ReleaseUnitsCountTextBox.Text = details.UnitsCount > 0 ? details.UnitsCount.ToString() : "1";

                    ActualFinalQuantityTextBox.TextChanged += ActualFinalQuantityTextBox_TextChanged;

                    RecalculateUnitPrice();
                }
            }
            catch { }
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

    private void ActualFinalQuantityTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        RecalculateUnitPrice();
    }

    private void RecalculateUnitPrice()
    {
        if (CalculatedUnitCostTextBlock == null || UnitPriceTextBox == null) return;

        if (TryParseDecimal(ActualFinalQuantityTextBox.Text, out var actualQty) && actualQty > 0)
        {
            var calculatedUnitCost = _selectedLotTotalCost > 0
                ? _selectedLotTotalCost / actualQty
                : 0m;

            CalculatedUnitCostTextBlock.Text = $"${calculatedUnitCost:F2}";

            if (string.IsNullOrWhiteSpace(UnitPriceTextBox.Text) || UnitPriceTextBox.Text == "0.00" || UnitPriceTextBox.Text == "0")
            {
                UnitPriceTextBox.Text = calculatedUnitCost.ToString("F2", CultureInfo.InvariantCulture);
            }
        }
        else
        {
            CalculatedUnitCostTextBlock.Text = "$ 0.00";
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

        var details = await ApiService.Instance.GetAsync<GetAgingLotDetailsDto>($"{AgingConstants.AGING_LOTS_ACTIVE}/{lotId}");
        if (details == null) return;

        var selectedLotName = (ActiveAgingLotsComboBox.SelectedItem as LookupItem)?.Name ?? string.Empty;
        var selectedChamberName = AgingLotsSummary.FirstOrDefault(l => l.LotId == lotId)?.ChamberName ?? string.Empty;
        var weighWindow = new LotReleaseWeighingWindow(
            selectedLotName,
            selectedChamberName,
            details.TotalBatchCost,
            details.UnitsCount
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

            var customLotName = ReleaseLotNameTextBox.Text?.Trim();
            var response = await ApiService.Instance.PostAndReadAsync(AgingConstants.AGING_LOTS_RELEASE, new ReleaseFromAgingRequest(
                lotId,
                warehouseId,
                weighWindow.TotalWeight,
                weighWindow.ValidUnitsCount,
                weighWindow.UnitPrice,
                string.IsNullOrWhiteSpace(customLotName) ? null : customLotName,
                selectedStorageLocationIds.Count > 0 ? selectedStorageLocationIds : null,
                weighWindow.ValidHeadWeights
            ));

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