using CraftFlow.Api.Modules.Catalog;
using CraftFlow.Api.Modules.Inventory;
using CraftFlow.Api.Modules.Production;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Dtos.Common;
using CraftFlow.SharedKernel.Dtos.Production;
using CraftFlow.SharedKernel.Enums;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Services;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace CraftFlow.Wpf.Pages.Production.Controls;

public partial class BrewhouseControl : UserControl
{
    public const string SUCCESS_LOAD = "UI_DATA_LOADED_SUCCESS";
    public const string ERROR_LOAD = "UI_DATA_LOAD_ERROR";
    public const string ERROR_API = "UI_API_ERROR_PREFIX";

    private const string DEFAULT_OVERHEAD_PERCENT = "15";
    private const string DEFAULT_UNITS_COUNT = "1";
    private const string DRAFT_PLAN_ENDPOINT = "api/production/batches/plan";

    public ObservableCollection<LookupItem> Warehouses { get; } = [];
    public ObservableCollection<LookupItem> Recipes { get; } = [];
    public ObservableCollection<LookupItem> ActiveBatches { get; } = [];
    public ObservableCollection<ActiveBatchWpfModel> ActiveBatchesSummary { get; } = [];
    public ObservableCollection<ActiveBatchWpfModel> PlannedBatchesSummary { get; } = [];

    private readonly ObservableCollection<RequirementCalculationDto> _requirements = [];
    private readonly DispatcherTimer _uiTimer = new();

    private decimal _currentRawCost = 0m;
    private bool _isInitializing = true;

    public TextBlock? StatusTextBlock { get; set; }
    public ProductionPage? ParentPage { get; set; }

    public BrewhouseControl()
    {
        InitializeComponent();

        BatchWarehouseComboBox.ItemsSource = Warehouses;
        DestinationWarehouseComboBox.ItemsSource = Warehouses;
        BatchRecipeComboBox.ItemsSource = Recipes;
        ActiveBatchComboBox.ItemsSource = ActiveBatches;
        RequirementsListBox.ItemsSource = _requirements;
        ActiveBatchesDataGrid.ItemsSource = ActiveBatchesSummary;

        _uiTimer.Interval = TimeSpan.FromSeconds(1);
        _uiTimer.Tick += UiTimer_Tick;

        Loaded += BrewhouseControl_Loaded;
        Unloaded += BrewhouseControl_Unloaded;
    }

    private void BrewhouseControl_Loaded(object sender, RoutedEventArgs e)
    {
        _uiTimer.Start();
    }

    private void BrewhouseControl_Unloaded(object sender, RoutedEventArgs e)
    {
        _uiTimer.Stop();
    }

    private void UiTimer_Tick(object? sender, EventArgs e)
    {
        foreach (var batch in ActiveBatchesSummary)
        {
            batch.CurrentElapsed = batch.CurrentElapsed.Add(TimeSpan.FromSeconds(1));
        }

        foreach (var planned in PlannedBatchesSummary)
        {
            planned.NotifyCountdownChanged();
        }
    }

    public async Task LoadDataAsync()
    {
        _isInitializing = true;
        try
        {
            var warehouses = await ApiService.Instance.GetAsync<List<LookupDto>>(InventoryConstants.WAREHOUSES);
            Warehouses.Clear();
            warehouses?.ForEach(w => Warehouses.Add(new LookupItem(w.Id, w.Name)));

            var recipes = await ApiService.Instance.GetAsync<List<LookupDto>>(CatalogConstants.RECIPES);
            Recipes.Clear();
            recipes?.ForEach(r => Recipes.Add(new LookupItem(r.Id, r.Name)));

            try
            {
                var summaryList = await ApiService.Instance.GetAsync<List<ActiveBatchSummaryDto>>(ProductionConstants.BATCHES_ACTIVE_SUMMARY);

                PlannedBatchesSummary.Clear();
                ActiveBatchesSummary.Clear();
                ActiveBatches.Clear();

                if (summaryList != null)
                {
                    var now = DateTime.UtcNow;
                    var draftIntStr = ((int)BatchState.Draft).ToString();

                    foreach (var b in summaryList)
                    {
                        var isDraft = string.Equals(b.State, BatchState.Draft.ToString(), StringComparison.OrdinalIgnoreCase)
                                   || b.State == draftIntStr;

                        var model = new ActiveBatchWpfModel
                        {
                            Id = b.Id,
                            BatchName = b.BatchName,
                            RecipeName = b.RecipeName,
                            StartedAt = b.StartedAt,
                            ScheduledAt = b.ScheduledAt,
                            TargetDurationMinutes = b.TargetDurationMinutes,
                            ElapsedMinutes = b.ElapsedMinutes,
                            IsOverdue = b.IsOverdue,
                            Status = b.State,
                            PlannedOutputQuantity = b.PlannedOutputQuantity,
                            IsDraft = isDraft,
                            CurrentElapsed = b.StartedAt.HasValue ? (now - b.StartedAt.Value) : TimeSpan.Zero
                        };

                        if (isDraft)
                        {
                            PlannedBatchesSummary.Add(model);
                        }
                        else
                        {
                            ActiveBatchesSummary.Add(model);
                            ActiveBatches.Add(new LookupItem(b.Id, b.BatchName));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SetStatusFormatted(ERROR_API, Brushes.Red, ex.Message);
                return;
            }

            if (BatchRecipeComboBox.SelectedIndex < 0 && Recipes.Count > 0)
                BatchRecipeComboBox.SelectedIndex = 0;

            if (BatchWarehouseComboBox.SelectedIndex < 0 && Warehouses.Count > 0)
                BatchWarehouseComboBox.SelectedIndex = 0;

            if (DestinationWarehouseComboBox.SelectedIndex < 0 && Warehouses.Count > 0)
                DestinationWarehouseComboBox.SelectedIndex = Warehouses.Count > 1 ? 1 : 0;

            GenerateDefaultBatchName();

            SetStatus(SUCCESS_LOAD, Brushes.Green);
        }
        catch (Exception ex)
        {
            SetStatusFormatted(ERROR_LOAD, Brushes.Red, ex.Message);
        }
        finally
        {
            _isInitializing = false;
            BatchInputs_Changed(this, null!);
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

        var defaultScheduled = DateTime.Now.AddHours(1);
        ScheduledDatePicker.SelectedDate = defaultScheduled.Date;
        ScheduledTimeTextBox.Text = defaultScheduled.ToString("HH:mm");
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
                $"{ProductionConstants.CALCULATE_REQUIREMENTS}?recipeId={recipeId}&warehouseId={warehouseId}&plannedQty={qtyStr}");

            _requirements.Clear();
            calc?.ForEach(_requirements.Add);

            var estimatedCost = await ApiService.Instance.GetAsync<decimal>(
                $"{ProductionConstants.ESTIMATE_COST}?recipeId={recipeId}&plannedQty={qtyStr}");

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

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(ProductionConstants.BATCHES_START, new
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

            if (ParentPage != null) await ParentPage.LoadDataExternalAsync();
        }
        else
        {
            SetStatusRaw(contentOrError, Brushes.Red);
        }
    }

    private async void PlanBatch_Click(object sender, RoutedEventArgs e)
    {
        if (BatchRecipeComboBox.SelectedValue is not Guid recipeId ||
            BatchWarehouseComboBox.SelectedValue is not Guid rawWarehouseId ||
            DestinationWarehouseComboBox.SelectedValue is not Guid destWarehouseId ||
            !TryParseDecimal(BatchQuantityTextBox.Text, out var plannedQuantity))
        {
            SetStatus("UI_INVALID_INPUT_FIELDS", Brushes.Red);
            return;
        }

        var selectedDate = ScheduledDatePicker.SelectedDate ?? DateTime.Today;
        if (!TimeSpan.TryParse(ScheduledTimeTextBox.Text.Trim(), out var selectedTime))
        {
            selectedTime = TimeSpan.FromHours(12);
        }

        var scheduledAtUtc = DateTime.SpecifyKind(selectedDate.Add(selectedTime), DateTimeKind.Local).ToUniversalTime();
        var customName = BatchNameTextBox.Text?.Trim();

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(DRAFT_PLAN_ENDPOINT, new
        {
            RecipeId = recipeId,
            WarehouseId = rawWarehouseId,
            DestinationWarehouseId = destWarehouseId,
            PlannedOutputQuantity = plannedQuantity,
            Name = string.IsNullOrWhiteSpace(customName) ? null : customName,
            ScheduledAt = scheduledAtUtc
        });

        if (isSuccess)
        {
            SetStatus("MSG_BATCH_PLANNED_SUCCESS", Brushes.Green);
            BatchNameTextBox.Clear();
            GenerateDefaultBatchName();

            if (ParentPage != null) await ParentPage.LoadDataExternalAsync();
        }
        else
        {
            SetStatusRaw(contentOrError, Brushes.Red);
        }
    }

    private async void StartDraftBatch_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Guid batchId)
        {
            var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync($"api/production/batches/{batchId}/start", new { });

            if (isSuccess)
            {
                SetStatus("UI_BATCH_STARTED_SUCCESS", Brushes.Green);
                if (ParentPage != null) await ParentPage.LoadDataExternalAsync();
            }
            else
            {
                SetStatusRaw(contentOrError, Brushes.Red);
            }
        }
    }

    private async void DeleteDraftBatch_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Guid batchId)
        {
            var (isSuccess, contentOrError) = await ApiService.Instance.DeleteAndReadAsync($"api/production/batches/{batchId}");

            if (isSuccess)
            {
                SetStatus("MSG_DRAFT_DELETED_SUCCESS", Brushes.OrangeRed);
                if (ParentPage != null) await ParentPage.LoadDataExternalAsync();
            }
            else
            {
                SetStatusRaw(contentOrError, Brushes.Red);
            }
        }
    }

    private void ActiveBatchesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ActiveBatchesDataGrid.SelectedItem is ActiveBatchWpfModel selectedBatch)
        {
            ActiveBatchComboBox.SelectedValue = selectedBatch.Id;
        }
    }

    private async void ActiveBatchComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ActiveBatchComboBox.SelectedValue is not Guid batchId || batchId == Guid.Empty) return;

        if (ActiveBatchComboBox.SelectedItem is LookupItem selectedItem)
        {
            CompleteBatchNameTextBox.Text = $"{selectedItem.Name} ({LocalizationService.Get("HEADER_PRODUCTS")})";
        }

        try
        {
            var endpoint = $"{ProductionConstants.PRODUCTION_COSTING}/{batchId}";
            var costData = await ApiService.Instance.GetAsync<BatchCostDto>(endpoint);

            if (costData != null)
            {
                _currentRawCost = costData.TotalRawMaterialCost;

                if (costData.OverheadPercentage.HasValue)
                {
                    UseOverheadCheckBox.IsChecked = true;
                    OverheadPercentageTextBox.Text = costData.OverheadPercentage.Value.ToString("F2", CultureInfo.InvariantCulture);
                }

                ActualOutputQuantityTextBox.Text = costData.ActualOutputQuantity > 0
                    ? costData.ActualOutputQuantity.ToString("F2", CultureInfo.InvariantCulture)
                    : string.Empty;

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

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(ProductionConstants.BATCHES_COMPLETE, new
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
            ActualUnitsCountTextBox.Text = DEFAULT_UNITS_COUNT;
            CompleteBatchNameTextBox.Clear();
            UseOverheadCheckBox.IsChecked = false;
            OverheadPercentageTextBox.Text = DEFAULT_OVERHEAD_PERCENT;

            if (ParentPage != null)
            {
                await ParentPage.LoadDataExternalAsync();

                var isReadyForAging = ParentPage.CompletedBatches.Any(b => b.Id == batchId);
                if (isReadyForAging)
                {
                    ParentPage.ProductionTabControl.SelectedIndex = 1;
                    ParentPage.AgingView.CompletedBatchesComboBox.SelectedValue = batchId;
                }
            }
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
                $"{ProductionConstants.CALCULATE_MAX_OUTPUT}?recipeId={recipeId}&warehouseId={warehouseId}");

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

    private void SetStatus(string resourceKey, Brush color)
    {
        if (StatusTextBlock == null) return;
        StatusTextBlock.Foreground = color;
        StatusTextBlock.Text = LocalizationService.Get(resourceKey);
    }

    private void SetStatusFormatted(string resourceKey, Brush color, params object[] args)
    {
        if (StatusTextBlock == null) return;
        StatusTextBlock.Foreground = color;
        var format = LocalizationService.Get(resourceKey);
        StatusTextBlock.Text = string.Format(format, args);
    }

    private void SetStatusRaw(string rawText, Brush color)
    {
        if (StatusTextBlock == null) return;
        StatusTextBlock.Foreground = color;
        StatusTextBlock.Text = rawText;
    }
}