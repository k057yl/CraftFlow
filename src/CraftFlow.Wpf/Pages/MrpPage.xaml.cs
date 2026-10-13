using CraftFlow.Api.Modules.Catalog;
using CraftFlow.Api.Modules.Inventory;
using CraftFlow.Api.Modules.Procurement;
using CraftFlow.Api.Modules.Production;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Dtos.Common;
using CraftFlow.SharedKernel.Dtos.MRP;
using CraftFlow.SharedKernel.Dtos.Production;
using CraftFlow.SharedKernel.Enums;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Services;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace CraftFlow.Wpf.Pages;

public sealed record MrpPurchaseItemRequestDto(
    Guid RawMaterialId,
    decimal Quantity
);

public sealed record CreateProcurementFromMrpCommand(
    Guid SupplierId,
    Guid WarehouseId,
    List<MrpPurchaseItemRequestDto> Items
);

public partial class MrpPage : Page
{
    private const string DRAFT_PLAN_ENDPOINT = "api/production/batches/plan";

    public ObservableCollection<MaterialRequirementDto> Requirements { get; } = [];
    public ObservableCollection<LookupItem> Suppliers { get; } = [];
    public ObservableCollection<LookupItem> Warehouses { get; } = [];
    public ObservableCollection<LookupItem> Recipes { get; } = [];
    public ObservableCollection<ActiveBatchWpfModel> PlannedBatchesSummary { get; } = [];

    private readonly DispatcherTimer _uiTimer = new();

    public MrpPage()
    {
        InitializeComponent();

        MrpDataGrid.ItemsSource = Requirements;
        SupplierComboBox.ItemsSource = Suppliers;
        WarehouseComboBox.ItemsSource = Warehouses;

        PlanBatchRecipeComboBox.ItemsSource = Recipes;
        PlanRawWarehouseComboBox.ItemsSource = Warehouses;
        PlanDestWarehouseComboBox.ItemsSource = Warehouses;

        _uiTimer.Interval = TimeSpan.FromSeconds(1);
        _uiTimer.Tick += UiTimer_Tick;

        Loaded += MrpPage_Loaded;
        Unloaded += MrpPage_Unloaded;
    }

    private async void MrpPage_Loaded(object sender, RoutedEventArgs e)
    {
        _uiTimer.Start();
        await LoadLookupsAsync();
        await LoadPlannedBatchesAsync();
    }

    private void MrpPage_Unloaded(object sender, RoutedEventArgs e)
    {
        _uiTimer.Stop();
    }

    private void UiTimer_Tick(object? sender, EventArgs e)
    {
        foreach (var planned in PlannedBatchesSummary)
        {
            planned.NotifyCountdownChanged();
        }
    }

    private async Task LoadLookupsAsync()
    {
        try
        {
            var suppliers = await ApiService.Instance.GetAsync<List<LookupDto>>(ProcurementConstants.SUPPLIERS);
            Suppliers.Clear();
            suppliers?.ForEach(s => Suppliers.Add(new LookupItem(s.Id, s.Name)));

            var warehouses = await ApiService.Instance.GetAsync<List<LookupDto>>(InventoryConstants.WAREHOUSES);
            Warehouses.Clear();
            warehouses?.ForEach(w => Warehouses.Add(new LookupItem(w.Id, w.Name)));

            var recipes = await ApiService.Instance.GetAsync<List<LookupDto>>(CatalogConstants.RECIPES);
            Recipes.Clear();
            recipes?.ForEach(r => Recipes.Add(new LookupItem(r.Id, r.Name)));

            if (SupplierComboBox.SelectedIndex < 0 && Suppliers.Count > 0)
                SupplierComboBox.SelectedIndex = 0;

            if (WarehouseComboBox.SelectedIndex < 0 && Warehouses.Count > 0)
                WarehouseComboBox.SelectedIndex = 0;

            if (PlanBatchRecipeComboBox.SelectedIndex < 0 && Recipes.Count > 0)
                PlanBatchRecipeComboBox.SelectedIndex = 0;

            if (PlanRawWarehouseComboBox.SelectedIndex < 0 && Warehouses.Count > 0)
                PlanRawWarehouseComboBox.SelectedIndex = 0;

            if (PlanDestWarehouseComboBox.SelectedIndex < 0 && Warehouses.Count > 0)
                PlanDestWarehouseComboBox.SelectedIndex = Warehouses.Count > 1 ? 1 : 0;

            GenerateDefaultPlanBatchName();
        }
        catch { }
    }

    private void GenerateDefaultPlanBatchName()
    {
        var shortCode = Guid.NewGuid().ToString()[..8].ToUpperInvariant();
        PlanBatchNameTextBox.Text = string.Concat(FormattingConstants.BATCH_PREFIX, shortCode);

        var defaultScheduled = DateTime.Now.AddHours(1);
        PlanScheduledDatePicker.SelectedDate = defaultScheduled.Date;
        PlanScheduledTimeTextBox.Text = defaultScheduled.ToString("HH:mm");
    }

    private async Task LoadPlannedBatchesAsync()
    {
        try
        {
            var summaryList = await ApiService.Instance.GetAsync<List<ActiveBatchSummaryDto>>(ProductionConstants.BATCHES_ACTIVE_SUMMARY);

            PlannedBatchesSummary.Clear();

            if (summaryList != null)
            {
                var draftIntStr = ((int)BatchState.Draft).ToString();

                foreach (var b in summaryList)
                {
                    var isDraft = string.Equals(b.State, BatchState.Draft.ToString(), StringComparison.OrdinalIgnoreCase)
                               || b.State == draftIntStr;

                    if (isDraft)
                    {
                        PlannedBatchesSummary.Add(new ActiveBatchWpfModel
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
                            IsDraft = true
                        });
                    }
                }
            }
        }
        catch { }
    }

    private async void CalculateMrp_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var report = await ApiService.Instance.GetMrpRequirementsAsync();

            Requirements.Clear();
            if (report.IsSuccess && report.Value != null)
            {
                report.Value.Requirements.ForEach(Requirements.Add);
                SetStatus(UiConstants.Messages.DATA_LOADED_SUCCESS, Brushes.Green);
            }
            else
            {
                SetStatus(UiConstants.Messages.DATA_LOAD_ERROR, Brushes.Red);
            }
        }
        catch (Exception ex)
        {
            SetStatus($"{UiConstants.Messages.DATA_LOAD_ERROR}: {ex.Message}", Brushes.Red);
        }
    }

    private async void CreatePurchaseOrder_Click(object sender, RoutedEventArgs e)
    {
        if (SupplierComboBox.SelectedValue is not Guid supplierId || supplierId == Guid.Empty)
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
            return;
        }

        if (WarehouseComboBox.SelectedValue is not Guid warehouseId || warehouseId == Guid.Empty)
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
            return;
        }

        var shortageItems = Requirements
            .Where(r => r.ShortageQuantity > 0)
            .Select(r => new MrpPurchaseItemRequestDto(r.RawMaterialId, r.ShortageQuantity))
            .ToList();

        if (shortageItems.Count == 0)
        {
            SetStatus(UiConstants.Messages.DATA_LOADED_SUCCESS, Brushes.Orange);
            return;
        }

        var command = new CreateProcurementFromMrpCommand(supplierId, warehouseId, shortageItems);

        (bool isSuccess, string contentOrError) = await ApiService.Instance.PostAndReadAsync("api/mrp/create-purchase-order", command);

        if (isSuccess)
        {
            SetStatus($"{UiConstants.Messages.ORDER_SHIPPED_SUCCESS} ORDER_ID: {contentOrError}", Brushes.Green);
            await ApiService.Instance.GetMrpRequirementsAsync();
        }
        else
        {
            SetStatus(contentOrError, Brushes.Red);
        }
    }

    private async void PlanBatch_Click(object sender, RoutedEventArgs e)
    {
        if (PlanBatchRecipeComboBox.SelectedValue is not Guid recipeId ||
            PlanRawWarehouseComboBox.SelectedValue is not Guid rawWarehouseId ||
            PlanDestWarehouseComboBox.SelectedValue is not Guid destWarehouseId ||
            !TryParseDecimal(PlanBatchQuantityTextBox.Text, out var plannedQuantity) || plannedQuantity <= 0)
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
            return;
        }

        var selectedDate = PlanScheduledDatePicker.SelectedDate ?? DateTime.Today;
        if (!TimeSpan.TryParse(PlanScheduledTimeTextBox.Text.Trim(), out var selectedTime))
        {
            selectedTime = TimeSpan.FromHours(12);
        }

        var scheduledAtUtc = DateTime.SpecifyKind(selectedDate.Add(selectedTime), DateTimeKind.Local).ToUniversalTime();
        var customName = PlanBatchNameTextBox.Text?.Trim();

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
            GenerateDefaultPlanBatchName();
            await LoadPlannedBatchesAsync();
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
                await LoadPlannedBatchesAsync();
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
                await LoadPlannedBatchesAsync();
            }
            else
            {
                SetStatusRaw(contentOrError, Brushes.Red);
            }
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

    private void SetStatus(string resourceKey, Brush color)
    {
        StatusTextBlock.Foreground = color;
        StatusTextBlock.Text = LocalizationService.Get(resourceKey);
    }

    private void SetStatusRaw(string rawText, Brush color)
    {
        StatusTextBlock.Foreground = color;
        StatusTextBlock.Text = rawText;
    }
}