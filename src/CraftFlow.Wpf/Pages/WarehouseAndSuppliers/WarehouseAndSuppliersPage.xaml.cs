using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Constants.Endpoints;
using CraftFlow.SharedKernel.Dtos.Catalog.Inventory;
using CraftFlow.SharedKernel.Dtos.Common;
using CraftFlow.SharedKernel.Dtos.Supplier;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Models.WarehouseAndSuppliers;
using CraftFlow.Wpf.Services;
using CraftFlow.Wpf.Windows;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CraftFlow.Wpf.Pages.WarehouseAndSuppliers;

public partial class WarehouseAndSuppliersPage : Page
{
    public ObservableCollection<LookupItem> FormSuppliers { get; } = [];
    public ObservableCollection<LookupItem> FilterSuppliers { get; } = [];

    public ObservableCollection<LookupItem> FormWarehouses { get; } = [];
    public ObservableCollection<LookupItem> FilterWarehouses { get; } = [];

    public ObservableCollection<LookupItem> RawMaterials { get; } = [];
    public ObservableCollection<StockLotGridDto> StockLots { get; } = [];
    public ObservableCollection<SupplierGridDto> SuppliersList { get; } = [];

    private bool _isDataLoaded = false;

    public WarehouseAndSuppliersPage() : this(null) { }

    public WarehouseAndSuppliersPage(object? dummy = null)
    {
        InitializeComponent();

        ReceiptControl.BindSources(FormSuppliers, FilterSuppliers, FormWarehouses, FilterWarehouses, RawMaterials, StockLots);
        SuppliersViewControl.BindSources(SuppliersList);

        ReceiptControl.StatusUpdated += SetStatusRaw;
        SuppliersViewControl.StatusUpdated += SetStatusRaw;

        ReceiptControl.FilterChangedRequested += LoadStockLotsAsync;
        ReceiptControl.StockChanged += RefreshCurrentStockAsync;
        SuppliersViewControl.SuppliersChanged += LoadDataAsync;

        Loaded += async (s, e) => await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            _isDataLoaded = false;

            bool canManageSuppliers = PermissionService.Instance.HasPermission(AppPermission.SUPPLIERS_MANAGE);
            SuppliersViewControl.ApplyPermissions(canManageSuppliers);

            var suppliers = await ApiService.Instance.GetAsync<List<SupplierDto>>($"{ProcurementConstants.SUPPLIERS}?includeInactive={SuppliersViewControl.IncludeInactive}") ?? [];

            FormSuppliers.Clear();
            FilterSuppliers.Clear();
            SuppliersList.Clear();

            FilterSuppliers.Add(new LookupItem(Guid.Empty, LocalizationService.Get(UiConstants.Filters.ALL_SUPPLIERS)));

            foreach (var s in suppliers)
            {
                if (s.IsActive)
                {
                    var lookupItem = new LookupItem(s.Id, s.Name);
                    FormSuppliers.Add(lookupItem);
                    FilterSuppliers.Add(lookupItem);
                }

                SuppliersList.Add(new SupplierGridDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    LegalName = s.LegalName,
                    TaxId = s.TaxId,
                    Phone = s.Phone,
                    Email = s.Email,
                    Address = s.Address,
                    Notes = s.Notes,
                    IsActive = s.IsActive
                });
            }

            var warehouses = await ApiService.Instance.GetAsync<List<LookupDto>>(InventoryConstants.WAREHOUSES) ?? [];
            FormWarehouses.Clear();
            FilterWarehouses.Clear();
            FilterWarehouses.Add(new LookupItem(Guid.Empty, LocalizationService.Get(UiConstants.Filters.ALL_WAREHOUSES)));
            foreach (var w in warehouses)
            {
                var item = new LookupItem(w.Id, w.Name);
                FormWarehouses.Add(item);
                FilterWarehouses.Add(item);
            }

            ReceiptControl.SetDefaultSelections();

            var raw = await ApiService.Instance.GetAsync<List<LookupDto>>($"{CatalogConstants.RAW_MATERIALS}?includeArchived=false") ?? [];
            RawMaterials.Clear();
            raw.ForEach(r => RawMaterials.Add(new LookupItem(r.Id, r.Name, r.Code)));

            _isDataLoaded = true;
            await RefreshCurrentStockAsync();

            SetStatusRaw(LocalizationService.Get(UiConstants.Messages.DATA_LOADED_SUCCESS), Brushes.Green);
        }
        catch (Exception ex)
        {
            var err = string.Format(LocalizationService.Get(UiConstants.Messages.DATA_LOAD_ERROR), ex.Message);
            SetStatusRaw(err, Brushes.Red);
            AlertWindow.ShowError(Window.GetWindow(this), ex.Message);
        }
    }

    private Task RefreshCurrentStockAsync()
    {
        var (supId, whId, expiring) = ReceiptControl.GetCurrentFilters();
        return LoadStockLotsAsync(supId, whId, expiring);
    }

    private async Task LoadStockLotsAsync(Guid? supplierId, Guid? warehouseId, bool onlyExpiringSoon)
    {
        if (!_isDataLoaded) return;

        try
        {
            var queryParams = new List<string> { $"onlyExpiringSoon={onlyExpiringSoon}" };
            if (supplierId.HasValue) queryParams.Add($"supplierId={supplierId.Value}");
            if (warehouseId.HasValue) queryParams.Add($"warehouseId={warehouseId.Value}");

            string queryUrl = $"{InventoryConstants.STOCK_LOTS}?{string.Join("&", queryParams)}";
            var lots = await ApiService.Instance.GetAsync<List<StockLotGridDto>>(queryUrl);

            StockLots.Clear();

            if (lots != null)
            {
                foreach (var lot in lots.Where(l => l.Quantity > 0m))
                {
                    StockLots.Add(lot);
                }
            }
        }
        catch { }
    }

    private void SetStatusRaw(string rawText, Brush color)
    {
        StatusTextBlock.Foreground = color;
        StatusTextBlock.Text = rawText;
    }
}