using CraftFlow.SharedKernel.Constants.Endpoints;
using CraftFlow.SharedKernel.Dtos.Catalog.Inventory;
using CraftFlow.SharedKernel.Dtos.Common;
using CraftFlow.SharedKernel.Dtos.Fermentation;
using CraftFlow.SharedKernel.Dtos.Production;
using CraftFlow.SharedKernel.Enums;
using CraftFlow.SharedKernel.Enums.Inventory;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Models.Fermentations;
using CraftFlow.Wpf.Services;
using CraftFlow.Wpf.Windows;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static CraftFlow.Wpf.Services.LocalizationService;

namespace CraftFlow.Wpf.Pages.Fermentation;

public partial class FermentationPage : Page
{
    public ObservableCollection<LookupItem> RawMaterialWarehouses { get; } = [];
    public ObservableCollection<LookupItem> WorkshopWarehouses { get; } = [];
    public ObservableCollection<LookupItem> DestinationWarehouses { get; } = [];
    public ObservableCollection<LookupItem> EquipmentLocations { get; } = [];
    public ObservableCollection<LookupItem> Recipes { get; } = [];

    public ObservableCollection<FermentationBatchUiModel> FermentationBatches { get; } = [];
    public ObservableCollection<FermentationReadingUiModel> SelectedBatchReadings { get; } = [];
    public ObservableCollection<LookupItem> ActiveProductionBatches { get; } = [];
    private readonly ObservableCollection<RequirementItemUiModel> _uiRequirements = [];

    private decimal _selectedBatchRawCost = 0m;
    private bool _isInitializing = true;

    public FermentationPage()
    {
        InitializeComponent();

        RecipeComboBox.ItemsSource = Recipes;
        RawWarehouseComboBox.ItemsSource = RawMaterialWarehouses;
        WorkshopWarehouseComboBox.ItemsSource = WorkshopWarehouses;
        DestinationWarehouseComboBox.ItemsSource = DestinationWarehouses;
        EquipmentLocationComboBox.ItemsSource = EquipmentLocations;
        RequirementsListBox.ItemsSource = _uiRequirements;

        FermentationBatchesDataGrid.ItemsSource = FermentationBatches;
        ReadingsHistoryListBox.ItemsSource = SelectedBatchReadings;

        FermentationDiscardView.StatusTextBlock = StatusTextBlock;
        FermentationDiscardView.ActiveBatches = ActiveProductionBatches;
        FermentationDiscardView.OnDataChangedAsync = LoadDataAsync;

        UpdateSaltCalculator();

        Loaded += async (s, e) => await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        _isInitializing = true;
        try
        {
            var warehouses = await ApiService.Instance.GetAsync<List<WarehouseDto>>(InventoryConstants.WAREHOUSES) ?? [];

            RawMaterialWarehouses.Clear();
            WorkshopWarehouses.Clear();
            DestinationWarehouses.Clear();

            foreach (var w in warehouses)
            {
                var item = new LookupItem(w.Id, w.Name);
                if (w.Type == WarehouseType.PRODUCTION_WORKSHOP)
                    WorkshopWarehouses.Add(item);
                else
                {
                    RawMaterialWarehouses.Add(item);
                    DestinationWarehouses.Add(item);
                }
            }

            if (WorkshopWarehouses.Count == 0)
                warehouses.ForEach(w => WorkshopWarehouses.Add(new LookupItem(w.Id, w.Name)));

            if (RawMaterialWarehouses.Count == 0)
                warehouses.ForEach(w => RawMaterialWarehouses.Add(new LookupItem(w.Id, w.Name)));

            var recipes = await ApiService.Instance.GetAsync<List<LookupDto>>($"{CatalogConstants.RECIPES}?includeArchived=false&module={AppModule.FERMENTATION}");
            Recipes.Clear();
            recipes?.ForEach(r => Recipes.Add(new LookupItem(r.Id, r.Name)));

            if (RecipeComboBox.SelectedIndex < 0 && Recipes.Count > 0)
                RecipeComboBox.SelectedIndex = 0;

            if (RawWarehouseComboBox.SelectedIndex < 0 && RawMaterialWarehouses.Count > 0)
                RawWarehouseComboBox.SelectedIndex = 0;

            if (WorkshopWarehouseComboBox.SelectedIndex < 0 && WorkshopWarehouses.Count > 0)
                WorkshopWarehouseComboBox.SelectedIndex = 0;

            if (DestinationWarehouseComboBox.SelectedIndex < 0 && DestinationWarehouses.Count > 0)
                DestinationWarehouseComboBox.SelectedIndex = DestinationWarehouses.Count > 1 ? 1 : 0;

            if (string.IsNullOrWhiteSpace(BatchNameTextBox.Text))
                GenerateDefaultBatchName();

            if (WorkshopWarehouseComboBox.SelectedValue is Guid selectedWorkshopId && selectedWorkshopId != Guid.Empty)
                await LoadEquipmentLocationsAsync(selectedWorkshopId);

            var selectedId = (FermentationBatchesDataGrid.SelectedItem as FermentationBatchUiModel)?.Id;
            var batches = await ApiService.Instance.GetAsync<List<FermentationBatchDto>>($"{FermentationConstants.BATCHES}?includeCompleted=false");

            FermentationBatches.Clear();
            ActiveProductionBatches.Clear();

            if (batches != null)
            {
                foreach (var b in batches)
                {
                    FermentationBatches.Add(new FermentationBatchUiModel
                    {
                        Id = b.Id,
                        BatchNumber = b.BatchNumber,
                        RecipeName = b.RecipeName,
                        SubstrateWeightKg = b.SubstrateWeightKg,
                        SaltPercentage = b.SaltPercentage,
                        CalculatedSaltGrams = b.CalculatedSaltGrams,
                        CurrentPh = b.CurrentPh,
                        VesselInfo = b.VesselInfo,
                        Stage = b.Stage,
                        DaysElapsed = b.DaysElapsed,
                        Readings = b.Readings
                    });

                    ActiveProductionBatches.Add(new LookupItem(b.Id, $"{b.BatchNumber} — {b.RecipeName}"));
                }
            }

            if (selectedId.HasValue)
            {
                FermentationBatchesDataGrid.SelectedItem = FermentationBatches.FirstOrDefault(x => x.Id == selectedId.Value);
            }
            else if (FermentationBatches.Count > 0)
            {
                FermentationBatchesDataGrid.SelectedIndex = 0;
            }
            else
            {
                SelectedBatchReadings.Clear();
                ClearCompletionForm();
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
            await RecalculateRequirementsAsync();
        }
    }

    private void GenerateDefaultBatchName()
    {
        var datePart = DateTime.Now.ToString("ddMMyy");
        var shortCode = Guid.NewGuid().ToString()[..4].ToUpperInvariant();
        BatchNameTextBox.Text = $"FERM-{datePart}-{shortCode}";
    }

    private async void WorkshopWarehouseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (WorkshopWarehouseComboBox.SelectedValue is Guid warehouseId && warehouseId != Guid.Empty)
            await LoadEquipmentLocationsAsync(warehouseId);
        else
            EquipmentLocations.Clear();
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
                    l.LocationType == StorageLocationType.BARREL
                    || l.LocationType == StorageLocationType.CONTAINER
                    || l.LocationType == StorageLocationType.VAT_TANK
                ).ToList();

                if (validLocations.Count == 0)
                    validLocations = locations;

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
                    EquipmentLocationComboBox.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            SetStatusFormatted("UI_ERR_DATA_LOAD_FMT", Brushes.Red, ex.Message);
        }
    }

    private async void QuickCreateEquipment_Click(object sender, RoutedEventArgs e)
    {
        var currentWarehouseId = WorkshopWarehouseComboBox.SelectedValue as Guid?;
        var dialog = new CreateStorageLocationWindow(currentWarehouseId)
        {
            Owner = Window.GetWindow(this)
        };

        if (dialog.ShowDialog() == true && currentWarehouseId.HasValue && currentWarehouseId != Guid.Empty)
            await LoadEquipmentLocationsAsync(currentWarehouseId.Value);
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

    private async void LaydownInputs_Changed(object sender, RoutedEventArgs e)
    {
        UpdateSaltCalculator();
        await RecalculateRequirementsAsync();
    }

    private void SaltInputs_Changed(object sender, TextChangedEventArgs e)
    {
        UpdateSaltCalculator();
    }

    private void UpdateSaltCalculator()
    {
        if (CalculatedSaltTextBlock == null || SubstrateWeightTextBox == null || SaltPercentageTextBox == null) return;

        if (TryParseDecimal(SubstrateWeightTextBox.Text, out var weightKg) && weightKg > 0 &&
            TryParseDecimal(SaltPercentageTextBox.Text, out var saltPct) && saltPct >= 0)
        {
            var grams = Math.Round(weightKg * 1000m * (saltPct / 100m), 1);
            CalculatedSaltTextBlock.Text = string.Format(Get("UI_FERM_LABEL_SALT_GRAMS_FMT"), grams);
        }
        else
        {
            CalculatedSaltTextBlock.Text = string.Format(Get("UI_FERM_LABEL_SALT_GRAMS_FMT"), 0m);
        }
    }

    private async Task RecalculateRequirementsAsync()
    {
        if (_isInitializing || EstimatedCostTextBlock == null || RequirementsListBox == null) return;

        if (RecipeComboBox?.SelectedValue is not Guid recipeId || recipeId == Guid.Empty ||
            RawWarehouseComboBox?.SelectedValue is not Guid warehouseId || warehouseId == Guid.Empty ||
            !TryParseDecimal(SubstrateWeightTextBox?.Text ?? string.Empty, out var plannedQty) || plannedQty <= 0)
        {
            _uiRequirements.Clear();
            EstimatedCostTextBlock.Text = "Сырьё: $0.00";
            return;
        }

        try
        {
            var qtyStr = plannedQty.ToString(CultureInfo.InvariantCulture);

            var calc = await ApiService.Instance.GetAsync<List<RequirementCalculationDto>>(
                $"{ProductionConstants.CALCULATE_REQUIREMENTS}?recipeId={recipeId}&warehouseId={warehouseId}&plannedQty={qtyStr}");

            _uiRequirements.Clear();
            if (calc != null)
            {
                foreach (var req in calc)
                {
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

            EstimatedCostTextBlock.Text = $"Сырьё: ${estimatedCost:F2}";
        }
        catch { }
    }

    private async void CalculateMaxQuantity_Click(object sender, RoutedEventArgs e)
    {
        if (RecipeComboBox.SelectedValue is not Guid recipeId ||
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
                SubstrateWeightTextBox.Text = roundedQty.ToString("0.##", CultureInfo.InvariantCulture);
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

    private async void CreateFermentationBatch_Click(object sender, RoutedEventArgs e)
    {
        if (RecipeComboBox.SelectedValue is not Guid recipeId || recipeId == Guid.Empty ||
            RawWarehouseComboBox.SelectedValue is not Guid rawWarehouseId || rawWarehouseId == Guid.Empty ||
            DestinationWarehouseComboBox.SelectedValue is not Guid destWarehouseId || destWarehouseId == Guid.Empty ||
            EquipmentLocationComboBox.SelectedValue is not Guid equipmentLocationId || equipmentLocationId == Guid.Empty ||
            !TryParseDecimal(SubstrateWeightTextBox.Text, out var substrateKg) || substrateKg <= 0 ||
            !TryParseDecimal(SaltPercentageTextBox.Text, out var saltPct) || saltPct < 0 ||
            !TryParseDecimal(TargetTempTextBox.Text, out var targetTemp))
        {
            SetStatus("UI_ERR_INVALID_INPUT_FIELDS", Brushes.Red);
            return;
        }

        decimal? initialPh = null;
        if (!string.IsNullOrWhiteSpace(InitialPhTextBox.Text))
        {
            if (TryParseDecimal(InitialPhTextBox.Text, out var parsedPh) && parsedPh > 0 && parsedPh <= 14)
            {
                initialPh = parsedPh;
            }
            else
            {
                SetStatus("UI_ERR_INVALID_INPUT_FIELDS", Brushes.Red);
                return;
            }
        }

        var customName = BatchNameTextBox.Text?.Trim();

        var (isSuccess, error) = await ApiService.Instance.PostAndReadAsync(FermentationConstants.BATCHES, new
        {
            RecipeId = recipeId,
            WarehouseId = rawWarehouseId,
            DestinationWarehouseId = destWarehouseId,
            EquipmentLocationId = equipmentLocationId,
            SubstrateWeightKg = substrateKg,
            SaltPercentage = saltPct,
            TargetTemperature = targetTemp,
            InitialPh = initialPh,
            BatchName = string.IsNullOrWhiteSpace(customName) ? null : customName
        });

        if (isSuccess)
        {
            InitialPhTextBox.Clear();
            GenerateDefaultBatchName();
            await LoadDataAsync();
            SetStatus("UI_FERM_MSG_PASSPORT_CREATED", Brushes.Green);
        }
        else
        {
            SetStatusRaw(error, Brushes.Red);
        }
    }

    private async void FermentationBatchesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SelectedBatchReadings.Clear();
        if (FermentationBatchesDataGrid.SelectedItem is not FermentationBatchUiModel selected)
        {
            ClearCompletionForm();
            return;
        }

        foreach (var r in selected.Readings)
        {
            var dateStr = r.RecordedAt.ToLocalTime().ToString("dd.MM HH:mm");
            var phStr = r.Ph.HasValue ? $"pH {r.Ph.Value:0.00}" : string.Empty;
            var tempStr = r.Temperature.HasValue ? $"{r.Temperature.Value:0.#}°C" : string.Empty;
            var actions = string.Join(", ", new[]
            {
                r.DegassedOrBurped ? "CO2" : null,
                r.BrineToppedUp ? "+Brine" : null
            }.Where(x => x != null));

            var parts = new[] { dateStr, phStr, tempStr, actions, r.SensoryNotes }
                .Where(p => !string.IsNullOrWhiteSpace(p));

            SelectedBatchReadings.Add(new FermentationReadingUiModel
            {
                Summary = string.Join(" | ", parts)
            });
        }

        CompleteBatchNameTextBox.Text = $"{selected.BatchNumber} ({Get("UI_BREWHOUSE_LABEL_FINISHED_PRODUCTS")})";
        ActualOutputQuantityTextBox.Text = selected.SubstrateWeightKg.ToString("0.##", CultureInfo.InvariantCulture);

        try
        {
            var costData = await ApiService.Instance.GetAsync<BatchCostDto>($"{ProductionConstants.PRODUCTION_COSTING}/{selected.Id}");
            _selectedBatchRawCost = costData?.TotalRawMaterialCost ?? 0m;
        }
        catch
        {
            _selectedBatchRawCost = 0m;
        }

        RecalculateCompletionCostUI();
    }

    private void ClearCompletionForm()
    {
        _selectedBatchRawCost = 0m;
        CompleteBatchNameTextBox.Clear();
        ActualOutputQuantityTextBox.Clear();
        ActualUnitsCountTextBox.Text = "1";
        RecalculateCompletionCostUI();
    }

    private void CostInputs_Changed(object sender, RoutedEventArgs e)
    {
        RecalculateCompletionCostUI();
    }

    private void RecalculateCompletionCostUI()
    {
        if (TotalBatchCostTextBlock == null || UnitCostTextBlock == null) return;

        decimal rawCost = _selectedBatchRawCost;
        decimal overheadPercent = 0m;

        if (UseOverheadCheckBox?.IsChecked == true && TryParseDecimal(OverheadPercentageTextBox.Text, out var parsedOverhead))
            overheadPercent = parsedOverhead;

        decimal totalBatchCost = rawCost + (rawCost * (overheadPercent / 100m));

        if (!TryParseDecimal(ActualOutputQuantityTextBox?.Text ?? string.Empty, out var actualWeight) || actualWeight <= 0)
            actualWeight = 1m;

        decimal unitCostPerKg = totalBatchCost / actualWeight;

        TotalBatchCostTextBlock.Text = $"${totalBatchCost:F2}";
        UnitCostTextBlock.Text = $"(${unitCostPerKg:F2} / {Get("UI_UNIT_KG")})";
    }

    private async void AddReading_Click(object sender, RoutedEventArgs e)
    {
        if (FermentationBatchesDataGrid.SelectedItem is not FermentationBatchUiModel selected)
        {
            SetStatus("UI_ERR_INVALID_INPUT_FIELDS", Brushes.Red);
            return;
        }

        decimal? ph = null;
        if (!string.IsNullOrWhiteSpace(ReadingPhTextBox.Text))
        {
            if (!TryParseDecimal(ReadingPhTextBox.Text, out var parsedPh) || parsedPh <= 0 || parsedPh > 14)
            {
                SetStatus("UI_ERR_INVALID_INPUT_FIELDS", Brushes.Red);
                return;
            }
            ph = parsedPh;
        }

        decimal? temp = null;
        if (!string.IsNullOrWhiteSpace(ReadingTempTextBox.Text))
        {
            if (!TryParseDecimal(ReadingTempTextBox.Text, out var parsedTemp))
            {
                SetStatus("UI_ERR_INVALID_INPUT_FIELDS", Brushes.Red);
                return;
            }
            temp = parsedTemp;
        }

        var url = $"{FermentationConstants.BATCHES}/{selected.Id}/{FermentationConstants.READINGS}";
        var (isSuccess, error) = await ApiService.Instance.PostAndReadAsync(url, new
        {
            Ph = ph,
            Temperature = temp,
            DegassedOrBurped = DegassedCheckBox.IsChecked == true,
            BrineToppedUp = BrineToppedUpCheckBox.IsChecked == true,
            SensoryNotes = string.IsNullOrWhiteSpace(SensoryNotesTextBox.Text) ? null : SensoryNotesTextBox.Text.Trim()
        });

        if (isSuccess)
        {
            ReadingPhTextBox.Clear();
            ReadingTempTextBox.Clear();
            DegassedCheckBox.IsChecked = false;
            BrineToppedUpCheckBox.IsChecked = false;
            SensoryNotesTextBox.Clear();

            await LoadDataAsync();
            SetStatus("UI_FERM_MSG_READING_ADDED", Brushes.Green);
        }
        else
        {
            SetStatusRaw(error, Brushes.Red);
        }
    }

    private async void MoveToCold_Click(object sender, RoutedEventArgs e)
    {
        if (FermentationBatchesDataGrid.SelectedItem is not FermentationBatchUiModel selected) return;

        var url = $"{FermentationConstants.BATCHES}/{selected.Id}/{FermentationConstants.MOVE_TO_COLD}";
        var (isSuccess, error) = await ApiService.Instance.PostAndReadAsync(url, new { });

        if (isSuccess)
        {
            await LoadDataAsync();
            SetStatus("UI_FERM_MSG_MOVED_COLD", Brushes.Green);
        }
        else
        {
            SetStatusRaw(error, Brushes.Red);
        }
    }

    private async void CompleteFermentation_Click(object sender, RoutedEventArgs e)
    {
        if (FermentationBatchesDataGrid.SelectedItem is not FermentationBatchUiModel selected) return;

        if (!TryParseDecimal(ActualOutputQuantityTextBox.Text, out var actualOutput) || actualOutput <= 0 ||
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
        var url = $"{FermentationConstants.BATCHES}/{selected.Id}/{FermentationConstants.COMPLETE}";

        var (isSuccess, error) = await ApiService.Instance.PostAndReadAsync(url, new
        {
            ActualOutputQuantity = actualOutput,
            UnitsCount = unitsCount,
            OverheadPercentage = overheadPercentage,
            BatchNumber = string.IsNullOrWhiteSpace(customBatchName) ? null : customBatchName
        });

        if (isSuccess)
        {
            UseOverheadCheckBox.IsChecked = false;
            OverheadPercentageTextBox.Text = "15";
            await LoadDataAsync();
            SetStatus("UI_FERM_MSG_COMPLETED", Brushes.Green);
        }
        else
        {
            SetStatusRaw(error, Brushes.Red);
        }
    }

    private void SetStatus(string resourceKey, Brush color)
    {
        StatusTextBlock.Foreground = color;
        StatusTextBlock.Text = Get(resourceKey);
    }

    private void SetStatusFormatted(string resourceKey, Brush color, params object[] args)
    {
        StatusTextBlock.Foreground = color;
        StatusTextBlock.Text = string.Format(Get(resourceKey), args);
    }

    private void SetStatusRaw(string rawText, Brush color)
    {
        StatusTextBlock.Foreground = color;
        StatusTextBlock.Text = rawText;
    }
}