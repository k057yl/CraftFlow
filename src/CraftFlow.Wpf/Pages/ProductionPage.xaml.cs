using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Services;

namespace CraftFlow.Wpf.Pages;

public partial class ProductionPage : Page
{
    public ObservableCollection<LookupItem> Warehouses { get; } = [];
    public ObservableCollection<LookupItem> Recipes { get; } = [];
    public ObservableCollection<LookupItem> ActiveBatches { get; } = [];

    public ProductionPage()
    {
        InitializeComponent();

        BatchWarehouseComboBox.ItemsSource = Warehouses;
        BatchRecipeComboBox.ItemsSource = Recipes;
        ActiveBatchComboBox.ItemsSource = ActiveBatches;

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

            SetStatus(UiConstants.Messages.DATA_LOADED_SUCCESS, Brushes.Green);
        }
        catch (Exception ex)
        {
            SetStatus($"{UiConstants.Messages.DATA_LOAD_ERROR}: {ex.Message}", Brushes.Red);
        }
    }

    private async void StartBatch_Click(object sender, RoutedEventArgs e)
    {
        if (BatchRecipeComboBox.SelectedValue is not Guid recipeId ||
            BatchWarehouseComboBox.SelectedValue is not Guid warehouseId ||
            !decimal.TryParse(BatchQuantityTextBox.Text, out var plannedQuantity))
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
            return;
        }

        var response = await ApiService.Instance.PostAsync(Endpoints.BATCHES_START, new
        {
            RecipeId = recipeId,
            WarehouseId = warehouseId,
            PlannedOutputQuantity = plannedQuantity
        });

        if (response.IsSuccessStatusCode)
        {
            var batchId = await response.Content.ReadFromJsonAsync<Guid>();
            SetStatus($"{UiConstants.Messages.BATCH_STARTED_SUCCESS} BATCH_ID: {batchId}", Brushes.Green);
            await LoadDataAsync();
        }
        else
        {
            SetStatus($"{UiConstants.Messages.API_ERROR_PREFIX}: {response.StatusCode}", Brushes.Red);
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

        var response = await ApiService.Instance.PostAsync(Endpoints.BATCHES_COMPLETE, new
        {
            BatchId = batchId,
            ActualOutputQuantity = actualOutput
        });

        if (response.IsSuccessStatusCode)
        {
            SetStatus(UiConstants.Messages.BATCH_COMPLETED_SUCCESS, Brushes.Green);
            await LoadDataAsync();
        }
        else
        {
            SetStatus($"{UiConstants.Messages.API_ERROR_PREFIX}: {response.StatusCode}", Brushes.Red);
        }
    }

    private async void CalculateCost_Click(object sender, RoutedEventArgs e)
    {
        if (ActiveBatchComboBox.SelectedValue is not Guid batchId)
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
            return;
        }

        try
        {
            var endpoint = $"{Endpoints.PRODUCTION_COSTING}/{batchId}";
            var costData = await ApiService.Instance.GetAsync<BatchCostDto>(endpoint);

            if (costData != null)
            {
                TotalCostTextBlock.Text = $"${costData.TotalRawMaterialCost:F2}";
                UnitCostTextBlock.Text = $"${costData.UnitCost:F2}";
                SetStatus(UiConstants.Messages.BATCH_COST_CALCULATED, Brushes.Green);
            }
        }
        catch (Exception ex)
        {
            SetStatus($"{UiConstants.Messages.DATA_LOAD_ERROR}: {ex.Message}", Brushes.Red);
        }
    }

    private void SetStatus(string msg, Brush color)
    {
        StatusTextBlock.Foreground = color;
        StatusTextBlock.Text = msg;
    }
}

public record BatchCostDto(
    Guid BatchId,
    string RecipeName,
    decimal PlannedOutputQuantity,
    decimal ActualOutputQuantity,
    decimal TotalRawMaterialCost,
    decimal UnitCost
    );