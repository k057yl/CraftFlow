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
    public string DisplayInfo => $"{MaterialName}: Нужно {RequiredQty:F2} | Доступно {AvailableQty:F2} " + (IsSufficient ? "✔" : "❌ (НЕ ХВАТАЕТ!)");
}

public partial class ProductionPage : Page
{
    public ObservableCollection<LookupItem> Warehouses { get; } = [];
    public ObservableCollection<LookupItem> Recipes { get; } = [];
    public ObservableCollection<LookupItem> ActiveBatches { get; } = [];

    private readonly ObservableCollection<RequirementCalculationDto> _requirements = [];

    public ProductionPage()
    {
        InitializeComponent();

        BatchWarehouseComboBox.ItemsSource = Warehouses;
        DestinationWarehouseComboBox.ItemsSource = Warehouses;
        BatchRecipeComboBox.ItemsSource = Recipes;
        ActiveBatchComboBox.ItemsSource = ActiveBatches;
        RequirementsListBox.ItemsSource = _requirements;

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
            if (calc != null)
            {
                calc.ForEach(_requirements.Add);
            }

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

            var endpoint = $"{Endpoints.PRODUCTION_COSTING}/{batchId}";
            var costData = await ApiService.Instance.GetAsync<BatchCostDto>(endpoint);
            if (costData != null)
            {
                TotalCostTextBlock.Text = $"${costData.TotalRawMaterialCost:F2}";
                UnitCostTextBlock.Text = $"${costData.UnitCost:F2}";
            }

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