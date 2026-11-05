using CraftFlow.Api.Modules.Catalog;
using CraftFlow.Api.Modules.Inventory;
using CraftFlow.Api.Modules.MRP;
using CraftFlow.Api.Modules.Procurement;
using CraftFlow.Api.Modules.Production;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Dtos.Common;
using CraftFlow.SharedKernel.Dtos.MRP;
using CraftFlow.SharedKernel.Dtos.Production;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Enums.Inventory;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Services;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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

public sealed class PlannedBatchSelectableWpfModel : ActiveBatchWpfModel
{
    private bool _isSelected = true;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }
    }
}

public partial class MrpPage : Page
{
    private const string DRAFT_PLAN_ENDPOINT = "api/production/batches/plan";

    public ObservableCollection<BatchMrpGroupDto> BatchGroups { get; } = [];
    public ObservableCollection<SummaryRequirementDto> SummaryRequirements { get; } = [];
    public ObservableCollection<LookupItem> Suppliers { get; } = [];
    public ObservableCollection<LookupItem> Warehouses { get; } = [];
    public ObservableCollection<LookupItem> Recipes { get; } = [];
    public ObservableCollection<PlannedBatchSelectableWpfModel> PlannedBatchesSummary { get; } = [];

    private readonly DispatcherTimer _uiTimer = new();
    private bool _isInitialLoading = true;
    private readonly HashSet<Guid> _mrpSelectedBatchIds = new();

    public MrpPage()
    {
        InitializeComponent();

        MrpTreeViewer.ItemsSource = BatchGroups;
        MrpSummaryDataGrid.ItemsSource = SummaryRequirements;

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
        if (!PermissionService.Instance.HasPermission(AppPermission.PROCUREMENT_VIEW) &&
            !PermissionService.Instance.HasPermission(AppPermission.PRODUCTION_VIEW))
        {
            SetStatus("UI_ERR_ACCESS_DENIED_MRP", Brushes.Red);
            return;
        }

        FilterFromDatePicker.SelectedDate = DateTime.Today.AddDays(-1);
        FilterToDatePicker.SelectedDate = DateTime.Today.AddDays(14);

        CollectionViewSource.GetDefaultView(PlannedBatchesSummary).Filter = item =>
        {
            if (item is PlannedBatchSelectableWpfModel batch)
                return !_mrpSelectedBatchIds.Contains(batch.Id);
            return true;
        };

        _uiTimer.Start();
        await LoadLookupsAsync();
        await LoadPlannedBatchesAsync();
        _isInitialLoading = false;
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
            if (PermissionService.Instance.HasPermission(AppPermission.SUPPLIERS_MANAGE) || PermissionService.Instance.HasPermission(AppPermission.PROCUREMENT_VIEW))
            {
                var suppliers = await ApiService.Instance.GetAsync<List<LookupDto>>(ProcurementConstants.SUPPLIERS);
                Suppliers.Clear();
                suppliers?.ForEach(s => Suppliers.Add(new LookupItem(s.Id, s.Name)));
            }

            var warehouses = await ApiService.Instance.GetAsync<List<LookupDto>>(InventoryConstants.WAREHOUSES);
            Warehouses.Clear();
            warehouses?.ForEach(w => Warehouses.Add(new LookupItem(w.Id, w.Name)));

            var recipes = await ApiService.Instance.GetAsync<List<LookupDto>>($"{CatalogConstants.RECIPES}?includeArchived=false");
            Recipes.Clear();
            recipes?.ForEach(r => Recipes.Add(new LookupItem(r.Id, r.Name)));

            if (SupplierComboBox.SelectedIndex < 0 && Suppliers.Count > 0) SupplierComboBox.SelectedIndex = 0;
            if (WarehouseComboBox.SelectedIndex < 0 && Warehouses.Count > 0) WarehouseComboBox.SelectedIndex = 0;
            if (PlanBatchRecipeComboBox.SelectedIndex < 0 && Recipes.Count > 0) PlanBatchRecipeComboBox.SelectedIndex = 0;
            if (PlanRawWarehouseComboBox.SelectedIndex < 0 && Warehouses.Count > 0) PlanRawWarehouseComboBox.SelectedIndex = 0;
            if (PlanDestWarehouseComboBox.SelectedIndex < 0 && Warehouses.Count > 0) PlanDestWarehouseComboBox.SelectedIndex = Warehouses.Count > 1 ? 1 : 0;

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
        if (!PermissionService.Instance.HasPermission(AppPermission.PRODUCTION_VIEW)) return;

        try
        {
            var summaryList = await ApiService.Instance.GetAsync<List<ActiveBatchSummaryDto>>(ProductionConstants.BATCHES_ACTIVE_SUMMARY);

            PlannedBatchesSummary.Clear();

            if (summaryList != null)
            {
                var draftIntStr = ((int)BatchState.DRAFT).ToString();
                var fromDate = FilterFromDatePicker?.SelectedDate ?? DateTime.MinValue;
                var toDate = FilterToDatePicker?.SelectedDate?.AddDays(1) ?? DateTime.MaxValue;

                foreach (var b in summaryList)
                {
                    var isDraft = string.Equals(b.State, BatchState.DRAFT.ToString(), StringComparison.OrdinalIgnoreCase) || b.State == draftIntStr;

                    if (isDraft)
                    {
                        var model = new PlannedBatchSelectableWpfModel
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
                            IsDraft = true,
                            IsSelected = false
                        };

                        PlannedBatchesSummary.Add(model);
                    }
                }
            }

            await CalculateMrpOnFlyAsync();
        }
        catch { }
    }

    private async Task CalculateMrpOnFlyAsync()
    {
        if (!PermissionService.Instance.HasPermission(AppPermission.PROCUREMENT_VIEW)) return;

        try
        {
            var selectedIds = _mrpSelectedBatchIds.ToList();

            var result = await ApiService.Instance.GetMrpRequirementsAsync(selectedIds);

            Dispatcher.Invoke(() =>
            {
                BatchGroups.Clear();
                SummaryRequirements.Clear();

                if (result.IsSuccess && result.Value != null)
                {
                    if (result.Value.Batches != null)
                    {
                        foreach (var b in result.Value.Batches)
                        {
                            BatchGroups.Add(b);
                        }
                    }

                    if (result.Value.SummaryRequirements != null)
                    {
                        foreach (var s in result.Value.SummaryRequirements)
                        {
                            SummaryRequirements.Add(s);
                        }
                    }

                    SetStatus(UiConstants.Messages.DATA_LOADED_SUCCESS, Brushes.Green);
                }
                else
                {
                    SetStatus(result.Error.Code, Brushes.Red);
                }
            });
        }
        catch (Exception ex)
        {
            SetStatusRaw(ex.Message, Brushes.Red);
        }
    }

    private async void ApplyFilter_Click(object sender, RoutedEventArgs e)
    {
        if (!_isInitialLoading && IsLoaded)
        {
            await LoadPlannedBatchesAsync();
        }
    }

    private async void MoveToMrp_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Guid batchId)
        {
            _mrpSelectedBatchIds.Add(batchId);
            CollectionViewSource.GetDefaultView(PlannedBatchesSummary).Refresh();
            await CalculateMrpOnFlyAsync();
        }
    }

    private async void RemoveFromMrp_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Guid batchId)
        {
            _mrpSelectedBatchIds.Remove(batchId);
            CollectionViewSource.GetDefaultView(PlannedBatchesSummary).Refresh();
            await CalculateMrpOnFlyAsync();
        }
    }

    private async void FilterDate_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (!_isInitialLoading && IsLoaded)
        {
            await LoadPlannedBatchesAsync();
        }
    }

    private async void SelectAllBatches_Click(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox cb)
        {
            bool isChecked = cb.IsChecked ?? false;
            foreach (var batch in PlannedBatchesSummary)
            {
                batch.IsSelected = isChecked;
            }
            await CalculateMrpOnFlyAsync();
        }
    }

    private async void CreatePurchaseOrder_Click(object sender, RoutedEventArgs e)
    {
        if (!PermissionService.Instance.HasPermission(AppPermission.PROCUREMENT_RECEIVE))
        {
            SetStatus("UI_ERR_ACCESS_DENIED_PURCHASE", Brushes.Red);
            return;
        }

        if (SupplierComboBox.SelectedValue is not Guid supplierId || supplierId == Guid.Empty ||
            WarehouseComboBox.SelectedValue is not Guid warehouseId || warehouseId == Guid.Empty)
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
            return;
        }

        var shortageItems = SummaryRequirements
            .Where(r => r.ShortageQuantity > 0)
            .Select(r => new MrpPurchaseItemRequestDto(r.RawMaterialId, r.ShortageQuantity))
            .ToList();

        if (shortageItems.Count == 0)
        {
            SetStatus(UiConstants.Messages.DATA_LOADED_SUCCESS, Brushes.Orange);
            return;
        }

        var command = new CreateProcurementFromMrpCommand(supplierId, warehouseId, shortageItems);

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(MrpConstants.MRP_CREATE_PURCHARE_ORDER, command);

        if (isSuccess)
        {
            var successTemplate = LocalizationService.Get("UI_ORDER_CREATED_WITH_ID");
            SetStatusRaw(string.Format(successTemplate, contentOrError), Brushes.Green);
            await CalculateMrpOnFlyAsync();
        }
        else
        {
            SetStatusRaw(contentOrError, Brushes.Red);
        }
    }

    private async void PlanBatch_Click(object sender, RoutedEventArgs e)
    {
        if (!PermissionService.Instance.HasPermission(AppPermission.PRODUCTION_EXECUTE))
        {
            SetStatus("UI_ERR_ACCESS_DENIED_PLAN", Brushes.Red);
            return;
        }

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
            SetStatus("UI_BATCH_PLANNED_SUCCESS", Brushes.Green);
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
        if (!PermissionService.Instance.HasPermission(AppPermission.PRODUCTION_EXECUTE)) return;

        if (sender is Button btn && btn.Tag is Guid batchId)
        {
            var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync($"api/production/batches/{batchId}/start", new { });

            if (isSuccess)
            {
                SetStatus(UiConstants.Messages.BATCH_STARTED_SUCCESS, Brushes.Green);
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
        if (!PermissionService.Instance.HasPermission(AppPermission.PRODUCTION_EXECUTE)) return;

        if (sender is Button btn && btn.Tag is Guid batchId)
        {
            var (isSuccess, contentOrError) = await ApiService.Instance.DeleteAndReadAsync($"api/production/batches/{batchId}");

            if (isSuccess)
            {
                SetStatus("UI_DRAFT_DELETED_SUCCESS", Brushes.OrangeRed);
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