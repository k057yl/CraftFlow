using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CraftFlow.Api.Modules.Inventory;
using CraftFlow.Api.Modules.Procurement;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Services;

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
    public ObservableCollection<MaterialRequirementDto> Requirements { get; } = [];
    public ObservableCollection<LookupItem> Suppliers { get; } = [];
    public ObservableCollection<LookupItem> Warehouses { get; } = [];

    public MrpPage()
    {
        InitializeComponent();
        MrpDataGrid.ItemsSource = Requirements;
        SupplierComboBox.ItemsSource = Suppliers;
        WarehouseComboBox.ItemsSource = Warehouses;

        Loaded += async (s, e) => await LoadLookupsAsync();
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

            if (SupplierComboBox.SelectedIndex < 0 && Suppliers.Count > 0)
                SupplierComboBox.SelectedIndex = 0;

            if (WarehouseComboBox.SelectedIndex < 0 && Warehouses.Count > 0)
                WarehouseComboBox.SelectedIndex = 0;
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

    private void SetStatus(string msg, Brush color)
    {
        StatusTextBlock.Foreground = color;
        StatusTextBlock.Text = LocalizationService.Get(msg);
    }
}