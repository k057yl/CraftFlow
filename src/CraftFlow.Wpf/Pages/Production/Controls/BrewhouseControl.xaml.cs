using CraftFlow.Api.Modules.Catalog;
using CraftFlow.Api.Modules.Inventory;
using CraftFlow.Api.Modules.Production;
using CraftFlow.SharedKernel.Dtos.Common;
using CraftFlow.SharedKernel.Dtos.Inventory;
using CraftFlow.SharedKernel.Dtos.Production;
using CraftFlow.SharedKernel.Enums.Inventory;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Services;
using CraftFlow.Wpf.Windows;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using static CraftFlow.Wpf.Services.LocalizationService;

namespace CraftFlow.Wpf.Pages.Production.Controls;

public partial class BrewhouseControl : UserControl
{
    public ObservableCollection<LookupItem> RawMaterialWarehouses { get; } = [];
    public ObservableCollection<LookupItem> BreweryWarehouses { get; } = [];
    public ObservableCollection<LookupItem> DestinationWarehouses { get; } = [];
    public ObservableCollection<LookupItem> EquipmentLocations { get; } = [];

    public ObservableCollection<LookupItem> Recipes { get; } = [];
    public ObservableCollection<LookupItem> ActiveBatches { get; } = [];
    public ObservableCollection<ActiveBatchWpfModel> ActiveBatchesSummary { get; } = [];

    public class RequirementItemUiModel
    {
        public string MaterialName { get; set; } = string.Empty;
        public decimal RequiredQty { get; set; }
        public decimal AvailableQty { get; set; }
        public bool IsSufficient { get; set; }

        public string FormattedInfo =>
            $"{MaterialName}: {Get("UI_BREWHOUSE_LABEL_REQ_NEED")} {RequiredQty:0.####} | {Get("UI_BREWHOUSE_LABEL_REQ_AVAIL")} {AvailableQty:0.####} {(IsSufficient ? "✔" : "❌")}";
    }

    private readonly ObservableCollection<RequirementItemUiModel> _uiRequirements = [];
    private readonly List<RequirementCalculationDto> _rawRequirements = [];
    private readonly DispatcherTimer _uiTimer = new();

    private decimal _currentRawCost = 0m;
    private bool _isInitializing = true;

    public TextBlock? StatusTextBlock { get; set; }
    public ProductionPage? ParentPage { get; set; }

    public BrewhouseControl()
    {
        InitializeComponent();

        RawWarehouseComboBox.ItemsSource = RawMaterialWarehouses;
        BreweryWarehouseComboBox.ItemsSource = BreweryWarehouses;
        DestinationWarehouseComboBox.ItemsSource = DestinationWarehouses;
        EquipmentLocationComboBox.ItemsSource = EquipmentLocations;

        BatchRecipeComboBox.ItemsSource = Recipes;
        ActiveBatchComboBox.ItemsSource = ActiveBatches;
        RequirementsListBox.ItemsSource = _uiRequirements;
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
    }

    public async Task LoadDataAsync()
    {
        _isInitializing = true;
        try
        {
            var warehouses = await ApiService.Instance.GetAsync<List<WarehouseDto>>(InventoryConstants.WAREHOUSES) ?? [];

            RawMaterialWarehouses.Clear();
            BreweryWarehouses.Clear();
            DestinationWarehouses.Clear();

            foreach (var w in warehouses)
            {
                var item = new LookupItem(w.Id, w.Name);

                if (w.Type == WarehouseType.BREWHOUSE)
                {
                    BreweryWarehouses.Add(item);
                }
                else
                {
                    RawMaterialWarehouses.Add(item);
                    DestinationWarehouses.Add(item);
                }
            }

            if (BreweryWarehouses.Count == 0)
            {
                warehouses.ForEach(w => BreweryWarehouses.Add(new LookupItem(w.Id, w.Name)));
            }
            if (RawMaterialWarehouses.Count == 0)
            {
                warehouses.ForEach(w => RawMaterialWarehouses.Add(new LookupItem(w.Id, w.Name)));
            }

            var recipes = await ApiService.Instance.GetAsync<List<LookupDto>>($"{CatalogConstants.RECIPES}?includeArchived=false");
            Recipes.Clear();
            recipes?.ForEach(r => Recipes.Add(new LookupItem(r.Id, r.Name)));

            try
            {
                var summaryList = await ApiService.Instance.GetAsync<List<ActiveBatchSummaryDto>>(ProductionConstants.BATCHES_ACTIVE_SUMMARY);

                ActiveBatchesSummary.Clear();
                ActiveBatches.Clear();

                if (summaryList != null)
                {
                    var now = DateTime.UtcNow;

                    foreach (var b in summaryList)
                    {
                        var isDraft = string.Equals(b.State, BatchState.DRAFT.ToString(), StringComparison.OrdinalIgnoreCase)
                                   || string.Equals(b.State, "Draft", StringComparison.OrdinalIgnoreCase)
                                   || string.Equals(b.State, "1", StringComparison.OrdinalIgnoreCase);

                        if (!isDraft)
                        {
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
                                IsDraft = false,
                                CurrentElapsed = b.StartedAt.HasValue ? (now - b.StartedAt.Value) : TimeSpan.Zero
                            };

                            ActiveBatchesSummary.Add(model);
                            ActiveBatches.Add(new LookupItem(b.Id, b.BatchName));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SetStatusFormatted("UI_BREWHOUSE_ERR_API_ACTIVE_BATCHES_FMT", Brushes.Red, ex.Message);
            }

            if (BatchRecipeComboBox.SelectedIndex < 0 && Recipes.Count > 0)
                BatchRecipeComboBox.SelectedIndex = 0;

            if (RawWarehouseComboBox.SelectedIndex < 0 && RawMaterialWarehouses.Count > 0)
                RawWarehouseComboBox.SelectedIndex = 0;

            if (BreweryWarehouseComboBox.SelectedIndex < 0 && BreweryWarehouses.Count > 0)
                BreweryWarehouseComboBox.SelectedIndex = 0;

            if (DestinationWarehouseComboBox.SelectedIndex < 0 && DestinationWarehouses.Count > 0)
                DestinationWarehouseComboBox.SelectedIndex = DestinationWarehouses.Count > 1 ? 1 : 0;

            GenerateDefaultBatchName();

            if (BreweryWarehouseComboBox.SelectedValue is Guid selectedBreweryId && selectedBreweryId != Guid.Empty)
            {
                await LoadEquipmentLocationsAsync(selectedBreweryId);
            }

            SetStatus("UI_MSG_DATA_LOADED_SUCCESS", Brushes.Green);
        }
        catch (Exception ex)
        {
            SetStatusFormatted("UI_ERR_DATA_LOAD_FMT", Brushes.Red, ex.Message);
        }
        finally
        {
            _isInitializing = false;
            BatchInputs_Changed(this, null!);
        }
    }

    private async void BreweryWarehouseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (BreweryWarehouseComboBox.SelectedValue is Guid breweryWarehouseId && breweryWarehouseId != Guid.Empty)
        {
            await LoadEquipmentLocationsAsync(breweryWarehouseId);
        }
        else
        {
            EquipmentLocations.Clear();
        }
    }

    private async Task LoadEquipmentLocationsAsync(Guid warehouseId)
    {
        try
        {
            var url = $"{InventoryConstants.WAREHOUSES_LOCATIONS}?warehouseId={warehouseId}";
            var locations = await ApiService.Instance.GetAsync<List<StorageLocationDto>>(url);

            EquipmentLocations.Clear();

            if (locations != null && locations.Count > 0)
            {
                var validLocations = locations.Where(l =>
                    l.LocationType == StorageLocationType.BREWING_VESSEL
                    || l.LocationType == StorageLocationType.VAT_TANK
                    || l.LocationType == StorageLocationType.CONTAINER
                ).ToList();

                if (validLocations.Count == 0)
                {
                    validLocations = locations;
                }

                var availableLocations = validLocations
                    .Where(l => l.Status == StorageLocationStatus.AVAILABLE || string.Equals(l.Status.ToString(), "AVAILABLE", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var defaultUnit = Get("UI_UNIT_LITERS");
                foreach (var l in availableLocations)
                {
                    var capacityText = l.Capacity.HasValue ? $" ({l.Capacity.Value:0.##} {l.UnitOfMeasure ?? defaultUnit})" : string.Empty;
                    EquipmentLocations.Add(new LookupItem(l.Id, $"{l.Name}{capacityText}"));
                }

                if (EquipmentLocations.Count > 0)
                {
                    EquipmentLocationComboBox.SelectedIndex = 0;
                }
            }
        }
        catch (Exception ex)
        {
            SetStatusFormatted("UI_BREWHOUSE_ERR_EQUIPMENT_LOAD_FMT", Brushes.Red, ex.Message);
        }
    }

    private async void QuickCreateEquipment_Click(object sender, RoutedEventArgs e)
    {
        Guid? currentWarehouseId = BreweryWarehouseComboBox.SelectedValue as Guid?;

        var dialog = new CreateStorageLocationWindow(currentWarehouseId)
        {
            Owner = Window.GetWindow(this)
        };

        if (dialog.ShowDialog() == true && currentWarehouseId.HasValue && currentWarehouseId != Guid.Empty)
        {
            await LoadEquipmentLocationsAsync(currentWarehouseId.Value);
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
        BatchNameTextBox.Text = string.Concat("BATCH-", shortCode);
    }

    private async void BatchInputs_Changed(object sender, RoutedEventArgs e)
    {
        if (_isInitializing || EstimatedCostTextBlock == null || RequirementsListBox == null) return;

        if (BatchRecipeComboBox?.SelectedValue is not Guid recipeId || recipeId == Guid.Empty ||
            RawWarehouseComboBox?.SelectedValue is not Guid warehouseId || warehouseId == Guid.Empty ||
            !TryParseDecimal(BatchQuantityTextBox?.Text ?? string.Empty, out var plannedQty) || plannedQty <= 0)
        {
            _uiRequirements.Clear();
            _rawRequirements.Clear();
            EstimatedCostTextBlock.Text = "$ 0.00";
            return;
        }

        try
        {
            var qtyStr = plannedQty.ToString(CultureInfo.InvariantCulture);

            var calc = await ApiService.Instance.GetAsync<List<RequirementCalculationDto>>(
                $"{ProductionConstants.CALCULATE_REQUIREMENTS}?recipeId={recipeId}&warehouseId={warehouseId}&plannedQty={qtyStr}");

            _uiRequirements.Clear();
            _rawRequirements.Clear();

            if (calc != null)
            {
                foreach (var req in calc)
                {
                    _rawRequirements.Add(req);
                    _uiRequirements.Add(new RequirementItemUiModel
                    {
                        MaterialName = req.MaterialName,
                        RequiredQty = req.RequiredQty,
                        AvailableQty = req.AvailableQty,
                        IsSufficient = req.IsSufficient
                    });
                }
            }

            var estimatedCost = await ApiService.Instance.GetAsync<decimal>(
                $"{ProductionConstants.ESTIMATE_COST}?recipeId={recipeId}&plannedQty={qtyStr}");

            EstimatedCostTextBlock.Text = $"${estimatedCost:F2}";
        }
        catch { }
    }

    private async void StartBatch_Click(object sender, RoutedEventArgs e)
    {
        if (BatchRecipeComboBox.SelectedValue is not Guid recipeId ||
            RawWarehouseComboBox.SelectedValue is not Guid rawWarehouseId ||
            DestinationWarehouseComboBox.SelectedValue is not Guid destWarehouseId ||
            EquipmentLocationComboBox.SelectedValue is not Guid equipmentLocationId ||
            !TryParseDecimal(BatchQuantityTextBox.Text, out var plannedQuantity))
        {
            SetStatus("UI_ERR_INVALID_INPUT_FIELDS", Brushes.Red);
            return;
        }

        var customName = BatchNameTextBox.Text?.Trim();

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(ProductionConstants.BATCHES_START, new
        {
            RecipeId = recipeId,
            WarehouseId = rawWarehouseId,
            DestinationWarehouseId = destWarehouseId,
            EquipmentLocationId = equipmentLocationId,
            PlannedOutputQuantity = plannedQuantity,
            Name = string.IsNullOrWhiteSpace(customName) ? null : customName
        });

        if (isSuccess)
        {
            SetStatus("UI_BREWHOUSE_MSG_BATCH_STARTED_SUCCESS", Brushes.Green);
            BatchNameTextBox.Clear();
            GenerateDefaultBatchName();

            await LoadDataAsync();

            if (ParentPage != null) await ParentPage.LoadDataExternalAsync();
        }
        else
        {
            SetStatusRaw(contentOrError, Brushes.Red);
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
            CompleteBatchNameTextBox.Text = $"{selectedItem.Name} ({Get("UI_BREWHOUSE_LABEL_FINISHED_PRODUCTS")})";
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
        UnitCostTextBlock.Text = $"${unitCostPerKg:F2} / {Get("UI_UNIT_KG")}";
    }

    private async void CompleteBatch_Click(object sender, RoutedEventArgs e)
    {
        if (ActiveBatchComboBox.SelectedValue is not Guid batchId ||
            !TryParseDecimal(ActualOutputQuantityTextBox.Text, out var actualOutput) || actualOutput <= 0 ||
            !int.TryParse(ActualUnitsCountTextBox.Text.Trim(), out var unitsCount) || unitsCount < 0)
        {
            SetStatus("UI_ERR_INVALID_INPUT_FIELDS", Brushes.Red);
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
                SetStatus("UI_ERR_INVALID_INPUT_FIELDS", Brushes.Red);
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
            SetStatus("UI_BREWHOUSE_MSG_BATCH_COMPLETED_SUCCESS", Brushes.Green);
            ActualOutputQuantityTextBox.Clear();
            ActualUnitsCountTextBox.Text = "1";
            CompleteBatchNameTextBox.Clear();
            UseOverheadCheckBox.IsChecked = false;
            OverheadPercentageTextBox.Text = "15";

            await LoadDataAsync();

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
            RawWarehouseComboBox.SelectedValue is not Guid warehouseId)
        {
            SetStatus("UI_ERR_INVALID_INPUT_FIELDS", Brushes.Red);
            return;
        }

        try
        {
            var maxQty = await ApiService.Instance.GetAsync<decimal>(
                $"{ProductionConstants.CALCULATE_MAX_OUTPUT}?recipeId={recipeId}&warehouseId={warehouseId}");

            if (maxQty > 0)
            {
                var roundedQty = Math.Round(maxQty, 2);

                BatchQuantityTextBox.Text = roundedQty.ToString("0.##", CultureInfo.InvariantCulture);

                BatchInputs_Changed(this, null!);

                SetStatusFormatted("UI_BREWHOUSE_MSG_MAX_CALCULATED_FMT", Brushes.Green, roundedQty);
            }
            else
            {
                SetStatus("UI_BREWHOUSE_ERR_INSUFFICIENT_RAW_MATERIAL", Brushes.OrangeRed);
            }
        }
        catch (Exception ex)
        {
            SetStatusFormatted("UI_BREWHOUSE_ERR_CALC_FMT", Brushes.Red, ex.Message);
        }
    }

    private void PrintRecipeCard_Click(object sender, RoutedEventArgs e)
    {
        if (_uiRequirements.Count == 0)
        {
            SetStatus("UI_BREWHOUSE_ERR_RECIPE_CARD_EMPTY", Brushes.Red);
            return;
        }

        var recipeName = (BatchRecipeComboBox.SelectedItem as LookupItem)?.Name ?? Get("UI_BREWHOUSE_LABEL_RECIPE_DEFAULT");
        var printDialog = new PrintDialog();

        if (printDialog.ShowDialog() == true)
        {
            var flowDocument = new FlowDocument
            {
                PagePadding = new Thickness(50),
                ColumnWidth = printDialog.PrintableAreaWidth
            };

            flowDocument.Blocks.Add(new Paragraph(
                new Run($"{Get("UI_BREWHOUSE_TITLE_RECIPE_CARD")}: {recipeName.ToUpper()}"))
            {
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center
            });

            flowDocument.Blocks.Add(new Paragraph(
                new Run($"{Get("UI_BREWHOUSE_LABEL_PRINT_DATE")}: {DateTime.Now:dd.MM.yyyy HH:mm} | {Get("UI_BREWHOUSE_COL_BATCH_NAME")}: {BatchNameTextBox.Text} | {Get("UI_BREWHOUSE_LABEL_VOLUME")}: {BatchQuantityTextBox.Text}"))
            {
                FontSize = 12,
                FontStyle = FontStyles.Italic,
                TextAlignment = TextAlignment.Center
            });

            var list = new List();
            foreach (var req in _uiRequirements)
            {
                list.ListItems.Add(new ListItem(
                    new Paragraph(
                        new Run($"{req.MaterialName}: {req.RequiredQty:0.####} ({Get("UI_BREWHOUSE_LABEL_REQ_AVAIL")} {req.AvailableQty:0.####})"))
                    { FontSize = 14 }));
            }

            flowDocument.Blocks.Add(list);

            var idp = ((IDocumentPaginatorSource)flowDocument).DocumentPaginator;
            printDialog.PrintDocument(idp, recipeName);
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