using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Services;

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
        RequiredQty,
        AvailableQty,
        IsSufficient ? FormattingConstants.CHECKMARK_SUFFICIENT : FormattingConstants.CHECKMARK_INSUFFICIENT
    );
}

public partial class ProductionPage : Page
{
    public ObservableCollection<LookupItem> Warehouses { get; } = [];
    public ObservableCollection<LookupItem> Recipes { get; } = [];
    public ObservableCollection<LookupItem> ActiveBatches { get; } = [];
    public ObservableCollection<LookupItem> CompletedBatches { get; } = [];
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

    private async Task LoadDataAsync()
    {
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

            var readyBatches = await ApiService.Instance.GetAsync<List<LookupDto>>("api/production/batches/ready-for-aging");
            CompletedBatches.Clear();
            readyBatches?.ForEach(b => CompletedBatches.Add(new LookupItem(b.Id, b.Name)));

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

            SetStatus(UiConstants.Messages.DATA_LOADED_SUCCESS, Brushes.Green);

            BatchInputs_Changed(this, null!);
        }
        catch (Exception ex)
        {
            SetStatus($"{UiConstants.Messages.DATA_LOAD_ERROR}: {ex.Message}", Brushes.Red);
        }
    }

    private async void BatchInputs_Changed(object sender, RoutedEventArgs e)
    {
        if (EstimatedCostTextBlock == null || RequirementsListBox == null) return;

        if (BatchRecipeComboBox?.SelectedValue is not Guid recipeId ||
            BatchWarehouseComboBox?.SelectedValue is not Guid warehouseId ||
            !decimal.TryParse(BatchQuantityTextBox?.Text, out var plannedQty) || plannedQty <= 0)
        {
            _requirements.Clear();
            EstimatedCostTextBlock.Text = "$ 0.00";
            return;
        }

        try
        {
            var calc = await ApiService.Instance.GetAsync<List<RequirementCalculationDto>>(
                $"api/production/calculate-requirements?recipeId={recipeId}&warehouseId={warehouseId}&plannedQty={plannedQty}");

            _requirements.Clear();
            calc?.ForEach(_requirements.Add);

            var estimatedCost = await ApiService.Instance.GetAsync<decimal>(
                $"api/production/estimate-cost?recipeId={recipeId}&plannedQty={plannedQty}");

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
            !decimal.TryParse(BatchQuantityTextBox.Text, out var plannedQuantity))
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
            return;
        }

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(Endpoints.BATCHES_START, new
        {
            RecipeId = recipeId,
            WarehouseId = rawWarehouseId,
            DestinationWarehouseId = destWarehouseId,
            PlannedOutputQuantity = plannedQuantity
        });

        if (isSuccess)
        {
            SetStatus($"{UiConstants.Messages.BATCH_STARTED_SUCCESS} BATCH_ID: {contentOrError}", Brushes.Green);
            await LoadDataAsync();
        }
        else
        {
            SetStatus(contentOrError, Brushes.Red);
        }
    }

    private async void ActiveBatchComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ActiveBatchComboBox.SelectedValue is not Guid batchId) return;

        try
        {
            var endpoint = $"{Endpoints.PRODUCTION_COSTING}/{batchId}";
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

    // Сценарий А: Завершить и оприходовать сразу на Склад ГП (без выдержки)
    private async void CompleteBatch_Click(object sender, RoutedEventArgs e)
    {
        if (ActiveBatchComboBox.SelectedValue is not Guid batchId ||
            !decimal.TryParse(ActualOutputQuantityTextBox.Text, out var actualOutput))
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
            return;
        }

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(Endpoints.BATCHES_COMPLETE, new
        {
            BatchId = batchId,
            ActualOutputQuantity = actualOutput
        });

        if (isSuccess)
        {
            SetStatus(UiConstants.Messages.BATCH_COMPLETED_SUCCESS, Brushes.Green);
            ActualOutputQuantityTextBox.Clear();
            await LoadDataAsync();
        }
        else
        {
            SetStatus(contentOrError, Brushes.Red);
        }
    }

    // Сценарий Б: Завершить варку И сразу переключить юзера на созревание
    private async void SendToAging_Click(object sender, RoutedEventArgs e)
    {
        if (ActiveBatchComboBox.SelectedValue is not Guid batchId ||
            !decimal.TryParse(ActualOutputQuantityTextBox.Text, out var actualOutput))
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
            return;
        }

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(Endpoints.BATCHES_COMPLETE, new
        {
            BatchId = batchId,
            ActualOutputQuantity = actualOutput
        });

        if (isSuccess)
        {
            SetStatus("Варка завершена! Выберите камеру созревания.", Brushes.Green);
            ActualOutputQuantityTextBox.Clear();

            await LoadDataAsync();

            ProductionTabControl.SelectedIndex = 1;

            CompletedBatchesComboBox.SelectedValue = batchId;
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

    private async void DiscardBatch_Click(object sender, RoutedEventArgs e)
    {
        if (DiscardBatchComboBox.SelectedValue is not Guid batchId ||
            string.IsNullOrWhiteSpace(DiscardReasonTextBox.Text))
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
            return;
        }

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync("api/production/batches/discard", new
        {
            BatchId = batchId,
            Reason = DiscardReasonTextBox.Text.Trim()
        });

        if (isSuccess)
        {
            SetStatus("Партия успешно списана в брак!", Brushes.OrangeRed);
            DiscardReasonTextBox.Clear();
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