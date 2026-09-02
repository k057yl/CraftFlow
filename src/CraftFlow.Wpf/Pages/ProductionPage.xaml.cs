using CraftFlow.SharedKernel.Constants;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Services;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace CraftFlow.Wpf.Pages;

public record BatchCostDto(
    Guid BatchId,
    string RecipeName,
    decimal PlannedOutputQuantity,
    decimal ActualOutputQuantity,
    decimal TotalRawMaterialCost,
    decimal UnitCost
);

public record RequirementCalculationDto(
    string MaterialName,
    decimal RequiredQty,
    decimal AvailableQty,
    bool IsSufficient
)
{
    public string DisplayInfo => string.Format(
        FormattingConstants.DISPLAY_INFO_REQUIREMENT_FORMAT,
        MaterialName,
        Math.Round(RequiredQty, 3),
        Math.Round(AvailableQty, 3),
        IsSufficient ? FormattingConstants.CHECKMARK_SUFFICIENT : FormattingConstants.CHECKMARK_INSUFFICIENT
    );
}

public record BatchReadyForAgingDto(Guid Id, string Name, int DefaultAgingDays);

public partial class ProductionPage : Page
{
    public ObservableCollection<LookupItem> Warehouses { get; } = [];
    public ObservableCollection<LookupItem> Recipes { get; } = [];
    public ObservableCollection<LookupItem> ActiveBatches { get; } = [];
    public ObservableCollection<BatchReadyForAgingDto> CompletedBatches { get; } = [];
    public ObservableCollection<LookupItem> AgingChambers { get; } = [];
    public ObservableCollection<LookupItem> ActiveAgingLots { get; } = [];

    private readonly ObservableCollection<RequirementCalculationDto> _requirements = [];

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

        Loaded += async (s, e) => await LoadDataAsync();
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

            var activeBatches = await ApiService.Instance.GetAsync<List<LookupDto>>(Endpoints.BATCHES_ACTIVE);
            ActiveBatches.Clear();
            activeBatches?.ForEach(b => ActiveBatches.Add(new LookupItem(b.Id, b.Name)));

            var readyBatches = await ApiService.Instance.GetAsync<List<BatchReadyForAgingDto>>(Endpoints.BATCHES_READY_AGING);
            CompletedBatches.Clear();
            readyBatches?.ForEach(CompletedBatches.Add);

            var chambers = await ApiService.Instance.GetAsync<List<LookupDto>>(Endpoints.AGING_CHAMBERS);
            AgingChambers.Clear();
            chambers?.ForEach(c => AgingChambers.Add(new LookupItem(c.Id, c.Name)));

            var activeLots = await ApiService.Instance.GetAsync<List<LookupDto>>(Endpoints.AGING_LOTS_ACTIVE);
            ActiveAgingLots.Clear();
            activeLots?.ForEach(l => ActiveAgingLots.Add(new LookupItem(l.Id, l.Name)));

            if (BatchRecipeComboBox.SelectedIndex < 0 && Recipes.Count > 0)
                BatchRecipeComboBox.SelectedIndex = 0;

            if (BatchWarehouseComboBox.SelectedIndex < 0 && Warehouses.Count > 0)
                BatchWarehouseComboBox.SelectedIndex = 0;

            if (DestinationWarehouseComboBox.SelectedIndex < 0 && Warehouses.Count > 0)
                DestinationWarehouseComboBox.SelectedIndex = Warehouses.Count > 1 ? 1 : 0;

            GenerateDefaultBatchName();

            SetStatus(UiConstants.Messages.DATA_LOADED_SUCCESS, Brushes.Green);
        }
        catch (Exception ex)
        {
            SetStatus($"{UiConstants.Messages.DATA_LOAD_ERROR}: {ex.Message}", Brushes.Red);
        }
        finally
        {
            _isInitializing = false;
            BatchInputs_Changed(this, null!);
        }
    }

    private void CompletedBatchesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CompletedBatchesComboBox.SelectedItem is BatchReadyForAgingDto selectedBatch)
        {
            MinAgingDaysTextBox.Text = selectedBatch.DefaultAgingDays.ToString();
            AgingLotNameTextBox.Text = $"{selectedBatch.Name} (Выдержка)";
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

        CompleteBatchNameTextBox.Text = $"{selectedBatch.Name} (Готовая продукция)";

        try
        {
            var endpoint = $"{Endpoints.PRODUCTION_COSTING}/{selectedBatch.Id}";
            var costData = await ApiService.Instance.GetAsync<BatchCostDto>(endpoint);

            if (costData != null)
            {
                TotalCostTextBlock.Text = $"${costData.TotalRawMaterialCost:F2}";
                UnitCostTextBlock.Text = $"${costData.UnitCost:F2}";
            }
        }
        catch
        {
        }
    }

    private void ActiveAgingLotsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ActiveAgingLotsComboBox.SelectedItem is LookupItem selectedLot)
        {
            ReleaseLotNameTextBox.Text = $"{selectedLot.Name} (Снято с выдержки)";
        }
        else
        {
            ReleaseLotNameTextBox.Clear();
        }
    }

    private bool _isInitializing = true;

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
        catch
        {
        }
    }

    private async void StartBatch_Click(object sender, RoutedEventArgs e)
    {
        if (BatchRecipeComboBox.SelectedValue is not Guid recipeId ||
            BatchWarehouseComboBox.SelectedValue is not Guid rawWarehouseId ||
            DestinationWarehouseComboBox.SelectedValue is not Guid destWarehouseId ||
            !TryParseDecimal(BatchQuantityTextBox.Text, out var plannedQuantity))
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
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
            SetStatus(UiConstants.Messages.BATCH_STARTED_SUCCESS, Brushes.Green);
            BatchNameTextBox.Clear();
            GenerateDefaultBatchName();
            await LoadDataAsync();
        }
        else
        {
            SetStatus(contentOrError, Brushes.Red);
        }
    }

    private async void CompleteBatch_Click(object sender, RoutedEventArgs e)
    {
        if (ActiveBatchComboBox.SelectedValue is not Guid batchId ||
            !TryParseDecimal(ActualOutputQuantityTextBox.Text, out var actualOutput) || actualOutput <= 0)
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
            return;
        }

        var customBatchName = CompleteBatchNameTextBox.Text?.Trim();

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(Endpoints.BATCHES_COMPLETE, new
        {
            BatchId = batchId,
            ActualOutputQuantity = actualOutput,
            BatchNumber = string.IsNullOrWhiteSpace(customBatchName) ? null : customBatchName
        });

        if (isSuccess)
        {
            SetStatus(UiConstants.Messages.BATCH_COMPLETED_SUCCESS, Brushes.Green);
            ActualOutputQuantityTextBox.Clear();
            CompleteBatchNameTextBox.Clear();

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
            SetStatus(contentOrError, Brushes.Red);
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
            SetStatus(UiConstants.Messages.LOT_TRANSFERRED_TO_AGING_SUCCESS, Brushes.Green);

            CompletedBatchesComboBox.SelectedIndex = -1;
            AgingLotNameTextBox.Clear();

            await LoadDataAsync();
        }
        else
        {
            SetStatus(contentOrError, Brushes.Red);
        }
    }

    private async void ReleaseFromAging_Click(object sender, RoutedEventArgs e)
    {
        if (ActiveAgingLotsComboBox.SelectedValue is not Guid lotId)
        {
            SetStatus(UiConstants.Messages.SELECT_AGING_LOT, Brushes.Red);
            return;
        }

        if (TargetWarehousesComboBox.SelectedValue is not Guid warehouseId)
        {
            SetStatus(UiConstants.Messages.SELECT_WAREHOUSE, Brushes.Red);
            return;
        }

        if (!TryParseDecimal(ActualFinalQuantityTextBox.Text, out var actualQty) || actualQty <= 0)
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
            return;
        }

        TryParseDecimal(UnitPriceTextBox.Text, out var unitPrice);
        var customLotName = ReleaseLotNameTextBox.Text?.Trim();

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(Endpoints.AGING_LOTS_RELEASE, new
        {
            AgingLotId = lotId,
            TargetWarehouseId = warehouseId,
            ActualFinalQuantity = actualQty,
            UnitPrice = unitPrice,
            CustomBatchNumber = string.IsNullOrWhiteSpace(customLotName) ? null : customLotName
        });

        if (isSuccess)
        {
            SetStatus(UiConstants.Messages.LOT_RELEASED_FROM_AGING_SUCCESS, Brushes.Green);
            ActualFinalQuantityTextBox.Clear();
            UnitPriceTextBox.Clear();
            ReleaseLotNameTextBox.Clear();
            await LoadDataAsync();
        }
        else
        {
            SetStatus($"{UiConstants.Messages.RELEASE_ERROR}: {contentOrError}", Brushes.Red);
        }
    }

    private async void DiscardBatch_Click(object sender, RoutedEventArgs e)
    {
        if (DiscardBatchComboBox.SelectedValue is not Guid batchId ||
            string.IsNullOrWhiteSpace(DiscardReasonTextBox.Text))
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
            return;
        }

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(Endpoints.BATCHES_DISCARD, new
        {
            BatchId = batchId,
            Reason = DiscardReasonTextBox.Text.Trim()
        });

        if (isSuccess)
        {
            SetStatus(UiConstants.Messages.BATCH_COMPLETED_SUCCESS, Brushes.OrangeRed);
            DiscardReasonTextBox.Clear();
            await LoadDataAsync();
        }
        else
        {
            SetStatus(contentOrError, Brushes.Red);
        }
    }

    private async void CalculateMaxQuantity_Click(object sender, RoutedEventArgs e)
    {
        if (BatchRecipeComboBox.SelectedValue is not Guid recipeId ||
            BatchWarehouseComboBox.SelectedValue is not Guid warehouseId)
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
            return;
        }

        try
        {
            var maxQty = await ApiService.Instance.GetAsync<decimal>(
                $"{Endpoints.CALCULATE_MAX_OUTPUT}?recipeId={recipeId}&warehouseId={warehouseId}");

            if (maxQty > 0)
            {
                BatchQuantityTextBox.Text = Math.Round(maxQty, 2).ToString("0.##");
                SetStatus($"Рассчитан максимальный объем: {Math.Round(maxQty, 2)} кг/л", Brushes.Green);
            }
            else
            {
                SetStatus("Недостаточно компонентов на складе для запуска варки!", Brushes.OrangeRed);
            }
        }
        catch (Exception ex)
        {
            SetStatus($"Ошибка расчета максимума: {ex.Message}", Brushes.Red);
        }
    }

    private void PrintRecipeCard_Click(object sender, RoutedEventArgs e)
    {
        if (_requirements.Count == 0)
        {
            SetStatus("Нет данных для печати тех. карты", Brushes.Red);
            return;
        }

        var recipeName = (BatchRecipeComboBox.SelectedItem as LookupItem)?.Name ?? "Рецептура";
        var printDialog = new PrintDialog();

        if (printDialog.ShowDialog() == true)
        {
            var flowDocument = new FlowDocument
            {
                PagePadding = new Thickness(50),
                ColumnWidth = printDialog.PrintableAreaWidth
            };

            flowDocument.Blocks.Add(new Paragraph(
                new Run($"ТЕХНОЛОГИЧЕСКАЯ КАРТА ВАРКИ: {recipeName.ToUpper()}"))
            {
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center
            });

            flowDocument.Blocks.Add(new Paragraph(
                new Run($"Дата: {DateTime.Now:dd.MM.yyyy HH:mm} | Партия: {BatchNameTextBox.Text} | План выхода: {BatchQuantityTextBox.Text} кг/л"))
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
                        new Run($"{req.MaterialName}: {req.RequiredQty:F3} кг/л (На складе: {req.AvailableQty:F3})"))
                    { FontSize = 14 }));
            }

            flowDocument.Blocks.Add(list);

            var idp = ((IDocumentPaginatorSource)flowDocument).DocumentPaginator;
            printDialog.PrintDocument(idp, $"ТехКарта_{recipeName}");
        }
    }

    private void SetStatus(string msg, Brush color)
    {
        StatusTextBlock.Foreground = color;
        StatusTextBlock.Text = LocalizationService.Get(msg);
    }
}