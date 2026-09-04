using CraftFlow.SharedKernel.Constants;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Models.Productions;
using CraftFlow.Wpf.Services;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace CraftFlow.Wpf.Pages;

public partial class ProductionPage : Page
{
    public ObservableCollection<LookupItem> Warehouses { get; } = [];
    public ObservableCollection<LookupItem> Recipes { get; } = [];
    public ObservableCollection<LookupItem> ActiveBatches { get; } = [];
    public ObservableCollection<ActiveBatchSummaryDto> ActiveBatchesSummary { get; } = [];
    public ObservableCollection<BatchReadyForAgingDto> CompletedBatches { get; } = [];
    public ObservableCollection<LookupItem> AgingChambers { get; } = [];
    public ObservableCollection<LookupItem> ActiveAgingLots { get; } = [];
    public ObservableCollection<AgingLotSummaryDto> AgingLotsSummary { get; } = [];

    private readonly ObservableCollection<RequirementCalculationDto> _requirements = [];
    private readonly DispatcherTimer _uiTimer = new();

    private decimal _selectedLotTotalCost;
    private string _selectedLotUnitName = FormattingConstants.DEFAULT_WEIGHT_UNIT;
    private decimal _currentRawCost = 0m;
    private bool _isInitializing = true;

    public ProductionPage()
    {
        InitializeComponent();

        BatchWarehouseComboBox.ItemsSource = Warehouses;
        DestinationWarehouseComboBox.ItemsSource = Warehouses;
        TargetWarehousesComboBox.ItemsSource = Warehouses;

        BatchRecipeComboBox.ItemsSource = Recipes;
        ActiveBatchComboBox.ItemsSource = ActiveBatches;
        CompletedBatchesComboBox.ItemsSource = CompletedBatches;
        DiscardBatchComboBox.ItemsSource = ActiveBatches;

        RequirementsListBox.ItemsSource = _requirements;
        AgingChambersComboBox.ItemsSource = AgingChambers;
        ActiveAgingLotsComboBox.ItemsSource = ActiveAgingLots;

        AgingLotsDataGrid.ItemsSource = AgingLotsSummary;
        ActiveBatchesDataGrid.ItemsSource = ActiveBatchesSummary;

        _uiTimer.Interval = TimeSpan.FromSeconds(1);
        _uiTimer.Tick += UiTimer_Tick;
        _uiTimer.Start();

        Loaded += async (s, e) => await LoadDataAsync();
    }

    private void UiTimer_Tick(object? sender, EventArgs e)
    {
        foreach (var batch in ActiveBatchesSummary)
        {
            batch.CurrentElapsed = batch.CurrentElapsed.Add(TimeSpan.FromSeconds(1));
        }
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

    private void GenerateDefaultBatchName()
    {
        var shortCode = Guid.NewGuid().ToString()[..8].ToUpperInvariant();
        BatchNameTextBox.Text = string.Concat(FormattingConstants.BATCH_PREFIX, shortCode);
    }

    private async Task LoadDataAsync()
    {
        _isInitializing = true;
        try
        {
            var warehouses = await ApiService.Instance.GetAsync<List<LookupDto>>(Endpoints.WAREHOUSES);
            Warehouses.Clear();
            warehouses?.ForEach(w => Warehouses.Add(new LookupItem(w.Id, w.Name)));

            var recipes = await ApiService.Instance.GetAsync<List<LookupDto>>(Endpoints.RECIPES);
            Recipes.Clear();
            recipes?.ForEach(r => Recipes.Add(new LookupItem(r.Id, r.Name)));

            var activeBatches = await ApiService.Instance.GetAsync<List<LookupDto>>(Endpoints.BATCHES_ACTIVE_SUMMARY);
            ActiveBatches.Clear();
            activeBatches?.ForEach(b => ActiveBatches.Add(new LookupItem(b.Id, b.Name)));

            try
            {
                var summaryList = await ApiService.Instance.GetAsync<List<ActiveBatchSummaryDto>>("api/production/batches/active-summary");
                ActiveBatchesSummary.Clear();
                ActiveBatches.Clear();
                if (summaryList != null)
                {
                    var now = DateTime.UtcNow;
                    foreach (var b in summaryList)
                    {
                        b.CurrentElapsed = now - b.StartedAt;
                        ActiveBatchesSummary.Add(b);

                        ActiveBatches.Add(new LookupItem(b.Id, b.BatchName));
                    }
                }
            }
            catch { }

            var readyBatches = await ApiService.Instance.GetAsync<List<BatchReadyForAgingDto>>(Endpoints.BATCHES_READY_AGING);
            CompletedBatches.Clear();
            readyBatches?.ForEach(CompletedBatches.Add);

            var chambers = await ApiService.Instance.GetAsync<List<LookupDto>>(Endpoints.AGING_CHAMBERS);
            AgingChambers.Clear();
            chambers?.ForEach(c => AgingChambers.Add(new LookupItem(c.Id, c.Name)));

            var activeLots = await ApiService.Instance.GetAsync<List<LookupDto>>(Endpoints.AGING_LOTS_ACTIVE);
            ActiveAgingLots.Clear();
            activeLots?.ForEach(l => ActiveAgingLots.Add(new LookupItem(l.Id, l.Name)));

            try
            {
                var activeSummary = await ApiService.Instance.GetAsync<List<AgingLotSummaryDto>>(Endpoints.AGING_LOTS_ACTIVE_SUMMARY);
                AgingLotsSummary.Clear();
                activeSummary?.ForEach(AgingLotsSummary.Add);
            }
            catch { }

            if (BatchRecipeComboBox.SelectedIndex < 0 && Recipes.Count > 0)
                BatchRecipeComboBox.SelectedIndex = 0;

            if (BatchWarehouseComboBox.SelectedIndex < 0 && Warehouses.Count > 0)
                BatchWarehouseComboBox.SelectedIndex = 0;

            if (DestinationWarehouseComboBox.SelectedIndex < 0 && Warehouses.Count > 0)
                DestinationWarehouseComboBox.SelectedIndex = Warehouses.Count > 1 ? 1 : 0;

            GenerateDefaultBatchName();

            SetStatus("UI_DATA_LOADED_SUCCESS", Brushes.Green);
        }
        catch (Exception ex)
        {
            SetStatusFormatted("UI_DATA_LOAD_ERROR", Brushes.Red, ex.Message);
        }
        finally
        {
            _isInitializing = false;
            BatchInputs_Changed(this, null!);
        }
    }

    private void ActiveBatchesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ActiveBatchesDataGrid.SelectedItem is ActiveBatchSummaryDto selectedBatch)
        {
            ActiveBatchComboBox.SelectedValue = selectedBatch.Id;
        }
    }

    private void CompletedBatchesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CompletedBatchesComboBox.SelectedItem is BatchReadyForAgingDto selectedBatch)
        {
            MinAgingDaysTextBox.Text = selectedBatch.DefaultAgingDays.ToString();
            AgingLotNameTextBox.Text = $"{selectedBatch.Name} ({LocalizationService.Get("NAV_AGING")})";
        }
        else
        {
            MinAgingDaysTextBox.Clear();
            AgingLotNameTextBox.Clear();
        }
    }

    private async void ActiveBatchComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ActiveBatchComboBox.SelectedItem is not LookupItem selectedBatch) return;

        CompleteBatchNameTextBox.Text = $"{selectedBatch.Name} ({LocalizationService.Get("HEADER_PRODUCTS")})";

        try
        {
            var endpoint = $"{Endpoints.PRODUCTION_COSTING}/{selectedBatch.Id}";
            var costData = await ApiService.Instance.GetAsync<BatchCostDto>(endpoint);

            if (costData != null)
            {
                _currentRawCost = costData.TotalRawMaterialCost;

                if (costData.OverheadPercentage.HasValue)
                {
                    UseOverheadCheckBox.IsChecked = true;
                    OverheadPercentageTextBox.Text = costData.OverheadPercentage.Value.ToString("F2", CultureInfo.InvariantCulture);
                }

                if (costData.ActualOutputQuantity > 0 && string.IsNullOrWhiteSpace(ActualOutputQuantityTextBox.Text))
                {
                    ActualOutputQuantityTextBox.Text = costData.ActualOutputQuantity.ToString("F2", CultureInfo.InvariantCulture);
                }

                RecalculateBatchCostUI();
            }
        }
        catch { }
    }

    private void CostInputs_Changed(object sender, RoutedEventArgs e)
    {
        RecalculateBatchCostUI();
    }

    private void RecalculateBatchCostUI()
    {
        if (RawMaterialCostTextBlock == null || TotalBatchCostTextBlock == null || UnitCostTextBlock == null) return;

        decimal rawCost = _currentRawCost;
        decimal overheadPercent = 0m;

        if (UseOverheadCheckBox.IsChecked == true && TryParseDecimal(OverheadPercentageTextBox.Text, out var parsedOverhead))
        {
            overheadPercent = parsedOverhead;
        }

        decimal overheadAmount = rawCost * (overheadPercent / 100m);
        decimal totalBatchCost = rawCost + overheadAmount;

        if (!TryParseDecimal(ActualOutputQuantityTextBox.Text, out var actualWeight) || actualWeight <= 0)
        {
            actualWeight = 1m;
        }

        decimal unitCostPerKg = totalBatchCost / actualWeight;

        RawMaterialCostTextBlock.Text = $"${rawCost:F2}";
        OverheadCostTextBlock.Text = $"+${overheadAmount:F2} ({overheadPercent:0.##}%)";
        TotalBatchCostTextBlock.Text = $"${totalBatchCost:F2}";
        UnitCostTextBlock.Text = $"${unitCostPerKg:F2} / {FormattingConstants.DEFAULT_WEIGHT_UNIT}";
    }

    private async void ActiveAgingLotsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ActiveAgingLotsComboBox.SelectedItem is LookupItem selectedLot)
        {
            ReleaseLotNameTextBox.Text = $"{selectedLot.Name} ({LocalizationService.Get("GROUP_RELEASE_AGING")})";

            try
            {
                var details = await ApiService.Instance.GetAsync<GetAgingLotDetailsDto>($"{Endpoints.AGING_LOTS_ACTIVE}/{selectedLot.Id}");
                if (details != null)
                {
                    _selectedLotTotalCost = details.TotalBatchCost;
                    _selectedLotUnitName = string.IsNullOrWhiteSpace(details.UnitName) ? FormattingConstants.DEFAULT_WEIGHT_UNIT : details.UnitName;

                    var costFormat = LocalizationService.Get("LABEL_CALCULATED_UNIT_COST");
                    CalculatedCostLabelTextBlock.Text = string.Format(costFormat, _selectedLotUnitName);

                    var priceFormat = LocalizationService.Get("LABEL_SELLING_PRICE_PER_UNIT");
                    UnitPriceLabelTextBlock.Text = string.Format(priceFormat, _selectedLotUnitName);

                    ActualFinalQuantityTextBox.Text = details.InitialQuantity.ToString("F2", CultureInfo.InvariantCulture);
                    ReleaseUnitsCountTextBox.Text = details.UnitsCount > 0 ? details.UnitsCount.ToString() : "1";

                    RecalculateUnitPrice();
                }
            }
            catch { }
        }
        else
        {
            ReleaseLotNameTextBox.Clear();
            ActualFinalQuantityTextBox.Clear();
            ReleaseUnitsCountTextBox.Text = "1";
            UnitPriceTextBox.Clear();
            CalculatedUnitCostTextBlock.Text = "$ 0.00";
            _selectedLotTotalCost = 0;

            CalculatedCostLabelTextBlock.Text = string.Format(LocalizationService.Get("LABEL_CALCULATED_UNIT_COST"), FormattingConstants.DEFAULT_WEIGHT_UNIT);
            UnitPriceLabelTextBlock.Text = string.Format(LocalizationService.Get("LABEL_SELLING_PRICE_PER_UNIT"), FormattingConstants.DEFAULT_WEIGHT_UNIT);
        }
    }

    private void ActualFinalQuantityTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        RecalculateUnitPrice();
    }

    private void RecalculateUnitPrice()
    {
        if (TryParseDecimal(ActualFinalQuantityTextBox.Text, out var actualQty) && actualQty > 0 && _selectedLotTotalCost > 0)
        {
            var calculatedUnitCost = _selectedLotTotalCost / actualQty;
            CalculatedUnitCostTextBlock.Text = $"${calculatedUnitCost:F2}";

            if (string.IsNullOrWhiteSpace(UnitPriceTextBox.Text) || UnitPriceTextBox.Text == "0.00")
            {
                UnitPriceTextBox.Text = calculatedUnitCost.ToString("F2", CultureInfo.InvariantCulture);
            }
        }
        else
        {
            CalculatedUnitCostTextBlock.Text = "$ 0.00";
        }
    }

    private async void BatchInputs_Changed(object sender, RoutedEventArgs e)
    {
        if (_isInitializing || EstimatedCostTextBlock == null || RequirementsListBox == null) return;

        if (BatchRecipeComboBox?.SelectedValue is not Guid recipeId || recipeId == Guid.Empty ||
            BatchWarehouseComboBox?.SelectedValue is not Guid warehouseId || warehouseId == Guid.Empty ||
            !TryParseDecimal(BatchQuantityTextBox?.Text ?? string.Empty, out var plannedQty) || plannedQty <= 0)
        {
            _requirements.Clear();
            EstimatedCostTextBlock.Text = "$ 0.00";
            return;
        }

        try
        {
            var qtyStr = plannedQty.ToString(CultureInfo.InvariantCulture);

            var calc = await ApiService.Instance.GetAsync<List<RequirementCalculationDto>>(
                $"{Endpoints.CALCULATE_REQUIREMENTS}?recipeId={recipeId}&warehouseId={warehouseId}&plannedQty={qtyStr}");

            _requirements.Clear();
            calc?.ForEach(_requirements.Add);

            var estimatedCost = await ApiService.Instance.GetAsync<decimal>(
                $"{Endpoints.ESTIMATE_COST}?recipeId={recipeId}&plannedQty={qtyStr}");

            EstimatedCostTextBlock.Text = $"${estimatedCost:F2}";
        }
        catch { }
    }

    private async void StartBatch_Click(object sender, RoutedEventArgs e)
    {
        if (BatchRecipeComboBox.SelectedValue is not Guid recipeId ||
            BatchWarehouseComboBox.SelectedValue is not Guid rawWarehouseId ||
            DestinationWarehouseComboBox.SelectedValue is not Guid destWarehouseId ||
            !TryParseDecimal(BatchQuantityTextBox.Text, out var plannedQuantity))
        {
            SetStatus("UI_INVALID_INPUT_FIELDS", Brushes.Red);
            return;
        }

        var customName = BatchNameTextBox.Text?.Trim();

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(Endpoints.BATCHES_START, new
        {
            RecipeId = recipeId,
            WarehouseId = rawWarehouseId,
            DestinationWarehouseId = destWarehouseId,
            PlannedOutputQuantity = plannedQuantity,
            Name = string.IsNullOrWhiteSpace(customName) ? null : customName
        });

        if (isSuccess)
        {
            SetStatus("UI_BATCH_STARTED_SUCCESS", Brushes.Green);
            BatchNameTextBox.Clear();
            GenerateDefaultBatchName();
            await LoadDataAsync();
        }
        else
        {
            SetStatusRaw(contentOrError, Brushes.Red);
        }
    }

    private async void CompleteBatch_Click(object sender, RoutedEventArgs e)
    {
        if (ActiveBatchComboBox.SelectedValue is not Guid batchId ||
            !TryParseDecimal(ActualOutputQuantityTextBox.Text, out var actualOutput) || actualOutput <= 0 ||
            !int.TryParse(ActualUnitsCountTextBox.Text.Trim(), out var unitsCount) || unitsCount < 0)
        {
            SetStatus("UI_INVALID_INPUT_FIELDS", Brushes.Red);
            return;
        }

        decimal? overheadPercentage = null;
        if (UseOverheadCheckBox.IsChecked == true)
        {
            if (TryParseDecimal(OverheadPercentageTextBox.Text, out var parsedOverhead) && parsedOverhead >= 0)
            {
                overheadPercentage = parsedOverhead;
            }
            else
            {
                SetStatus("UI_INVALID_INPUT_FIELDS", Brushes.Red);
                return;
            }
        }

        var customBatchName = CompleteBatchNameTextBox.Text?.Trim();

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(Endpoints.BATCHES_COMPLETE, new
        {
            BatchId = batchId,
            ActualOutputQuantity = actualOutput,
            UnitsCount = unitsCount,
            BatchNumber = string.IsNullOrWhiteSpace(customBatchName) ? null : customBatchName,
            OverheadPercentage = overheadPercentage
        });

        if (isSuccess)
        {
            SetStatus("UI_BATCH_COMPLETED_SUCCESS", Brushes.Green);
            ActualOutputQuantityTextBox.Clear();
            ActualUnitsCountTextBox.Text = "1";
            CompleteBatchNameTextBox.Clear();
            UseOverheadCheckBox.IsChecked = false;
            OverheadPercentageTextBox.Text = "15";

            await LoadDataAsync();

            var isReadyForAging = CompletedBatches.Any(b => b.Id == batchId);

            if (isReadyForAging)
            {
                ProductionTabControl.SelectedIndex = 1;
                CompletedBatchesComboBox.SelectedValue = batchId;
            }
        }
        else
        {
            SetStatusRaw(contentOrError, Brushes.Red);
        }
    }

    private async void TransferToAging_Click(object sender, RoutedEventArgs e)
    {
        if (CompletedBatchesComboBox.SelectedValue is not Guid batchId ||
            AgingChambersComboBox.SelectedValue is not Guid chamberId ||
            !int.TryParse(MinAgingDaysTextBox.Text.Trim(), out var minDays))
        {
            SetStatus("UI_INVALID_INPUT_FIELDS", Brushes.Red);
            return;
        }

        var customLotName = AgingLotNameTextBox.Text?.Trim();

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(Endpoints.AGING_LOTS_TRANSFER, new
        {
            ProductionBatchId = batchId,
            AgingChamberId = chamberId,
            MinAgingDays = minDays,
            CustomBatchNumber = string.IsNullOrWhiteSpace(customLotName) ? null : customLotName
        });

        if (isSuccess)
        {
            SetStatus("UI_BATCH_STARTED_SUCCESS", Brushes.Green);

            CompletedBatchesComboBox.SelectedIndex = -1;
            AgingLotNameTextBox.Clear();

            await LoadDataAsync();
        }
        else
        {
            SetStatusRaw(contentOrError, Brushes.Red);
        }
    }

    private async void ReleaseFromAging_Click(object sender, RoutedEventArgs e)
    {
        if (ActiveAgingLotsComboBox.SelectedValue is not Guid lotId)
        {
            SetStatus("LABEL_ACTIVE_AGING_LOT", Brushes.Red);
            return;
        }

        if (TargetWarehousesComboBox.SelectedValue is not Guid warehouseId)
        {
            SetStatus("SELECT_WAREHOUSE", Brushes.Red);
            return;
        }

        if (!TryParseDecimal(ActualFinalQuantityTextBox.Text, out var actualQty) || actualQty <= 0)
        {
            SetStatus("UI_INVALID_INPUT_FIELDS", Brushes.Red);
            return;
        }

        if (!int.TryParse(ReleaseUnitsCountTextBox.Text.Trim(), out var unitsCount) || unitsCount <= 0)
        {
            SetStatus("UI_INVALID_INPUT_FIELDS", Brushes.Red);
            return;
        }

        TryParseDecimal(UnitPriceTextBox.Text, out var unitPrice);
        var customLotName = ReleaseLotNameTextBox.Text?.Trim();

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(Endpoints.AGING_LOTS_RELEASE, new
        {
            AgingLotId = lotId,
            TargetWarehouseId = warehouseId,
            ActualFinalQuantity = actualQty,
            UnitsCount = unitsCount,
            UnitPrice = unitPrice,
            CustomBatchNumber = string.IsNullOrWhiteSpace(customLotName) ? null : customLotName
        });

        if (isSuccess)
        {
            SetStatus("UI_ORDER_SHIPPED_SUCCESS", Brushes.Green);
            ActualFinalQuantityTextBox.Clear();
            ReleaseUnitsCountTextBox.Clear();
            UnitPriceTextBox.Clear();
            ReleaseLotNameTextBox.Clear();
            await LoadDataAsync();
        }
        else
        {
            SetStatusFormatted("UI_API_ERROR_PREFIX", Brushes.Red, contentOrError);
        }
    }

    private async void DiscardBatch_Click(object sender, RoutedEventArgs e)
    {
        if (DiscardBatchComboBox.SelectedValue is not Guid batchId ||
            string.IsNullOrWhiteSpace(DiscardReasonTextBox.Text))
        {
            SetStatus("UI_INVALID_INPUT_FIELDS", Brushes.Red);
            return;
        }

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(Endpoints.BATCHES_DISCARD, new
        {
            BatchId = batchId,
            Reason = DiscardReasonTextBox.Text.Trim()
        });

        if (isSuccess)
        {
            SetStatus("UI_BATCH_COMPLETED_SUCCESS", Brushes.OrangeRed);
            DiscardReasonTextBox.Clear();
            await LoadDataAsync();
        }
        else
        {
            SetStatusRaw(contentOrError, Brushes.Red);
        }
    }

    private async void CalculateMaxQuantity_Click(object sender, RoutedEventArgs e)
    {
        if (BatchRecipeComboBox.SelectedValue is not Guid recipeId ||
            BatchWarehouseComboBox.SelectedValue is not Guid warehouseId)
        {
            SetStatus("UI_INVALID_INPUT_FIELDS", Brushes.Red);
            return;
        }

        try
        {
            var maxQty = await ApiService.Instance.GetAsync<decimal>(
                $"{Endpoints.CALCULATE_MAX_OUTPUT}?recipeId={recipeId}&warehouseId={warehouseId}");

            if (maxQty > 0)
            {
                BatchQuantityTextBox.Text = Math.Round(maxQty, 2).ToString("0.##");
                SetStatusFormatted("LABEL_PLANNED_OUTPUT", Brushes.Green, Math.Round(maxQty, 2));
            }
            else
            {
                SetStatus("PRODUCTION_INSUFFICIENT_RAW_MATERIAL", Brushes.OrangeRed);
            }
        }
        catch (Exception ex)
        {
            SetStatusFormatted("UI_API_ERROR_PREFIX", Brushes.Red, ex.Message);
        }
    }

    private void PrintRecipeCard_Click(object sender, RoutedEventArgs e)
    {
        if (_requirements.Count == 0)
        {
            SetStatus("GENERAL_NOT_FOUND", Brushes.Red);
            return;
        }

        var recipeName = (BatchRecipeComboBox.SelectedItem as LookupItem)?.Name ?? LocalizationService.Get("RECIPE_NAME");
        var printDialog = new PrintDialog();

        if (printDialog.ShowDialog() == true)
        {
            var flowDocument = new FlowDocument
            {
                PagePadding = new Thickness(50),
                ColumnWidth = printDialog.PrintableAreaWidth
            };

            flowDocument.Blocks.Add(new Paragraph(
                new Run($"{LocalizationService.Get("HEADER_RECIPES")}: {recipeName.ToUpper()}"))
            {
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center
            });

            flowDocument.Blocks.Add(new Paragraph(
                new Run($"Дата: {DateTime.Now:dd.MM.yyyy HH:mm} | {LocalizationService.Get("LABEL_BATCH_CODE")} {BatchNameTextBox.Text} | {LocalizationService.Get("LABEL_PLANNED_OUTPUT")} {BatchQuantityTextBox.Text}"))
            {
                FontSize = 12,
                FontStyle = FontStyles.Italic,
                TextAlignment = TextAlignment.Center
            });

            var list = new List();
            foreach (var req in _requirements)
            {
                list.ListItems.Add(new ListItem(
                    new Paragraph(
                        new Run($"{req.MaterialName}: {req.RequiredQty:F3} ({LocalizationService.Get("AVAILABLE_STOCK")} {req.AvailableQty:F3})"))
                    { FontSize = 14 }));
            }

            flowDocument.Blocks.Add(list);

            var idp = ((IDocumentPaginatorSource)flowDocument).DocumentPaginator;
            printDialog.PrintDocument(idp, $"TechCard_{recipeName}");
        }
    }

    private void AgingLotsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (AgingLotsDataGrid.SelectedItem is AgingLotSummaryDto selectedLot)
        {
            ActiveAgingLotsComboBox.SelectedValue = selectedLot.LotId;
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