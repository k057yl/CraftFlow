using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Constants.Endpoints;
using CraftFlow.SharedKernel.Dtos.Aging;
using CraftFlow.SharedKernel.Dtos.Catalog.Inventory;
using CraftFlow.SharedKernel.Dtos.Common;
using CraftFlow.SharedKernel.Dtos.Supplier;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Enums.Inventory;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Models.Inventory;
using CraftFlow.Wpf.Services;
using CraftFlow.Wpf.Windows;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace CraftFlow.Wpf.Pages;

public partial class ProcurementPage : Page
{
    private const string UI_SUPPLIERS_MANAGE_REQUIRED = "UI_ERR_PERMISSION_SUPPLIERS_MANAGE_REQUIRED";
    private const string UI_SUPPLIER_UPDATED_SUCCESS = "UI_MSG_SUPPLIER_UPDATED_SUCCESS";
    private const string UI_HEADER_SUPPLIER_EDIT = "UI_HEADER_SUPPLIER_EDIT";
    private const string UI_BTN_SAVE_CHANGES = "UI_BTN_SAVE_CHANGES";

    public ObservableCollection<LookupItem> FormSuppliers { get; } = [];
    public ObservableCollection<LookupItem> FilterSuppliers { get; } = [];

    public ObservableCollection<LookupItem> FormWarehouses { get; } = [];
    public ObservableCollection<LookupItem> FilterWarehouses { get; } = [];

    public ObservableCollection<LookupItem> RawMaterials { get; } = [];
    public ObservableCollection<StockLotGridDto> StockLots { get; } = [];
    public ObservableCollection<SupplierGridDto> SuppliersList { get; } = [];

    public ObservableCollection<StorageLocationDto> AvailableLocations { get; } = [];
    public ObservableCollection<StorageLocationDto> SelectedLocations { get; } = [];
    public ObservableCollection<ReceiptDraftItem> DraftItems { get; } = [];

    private bool _isDataLoaded = false;
    private Guid? _editingSupplierId = null;

    public ProcurementPage() : this(null)
    {
    }

    public ProcurementPage(object? dummy = null)
    {
        InitializeComponent();

        StockSupplierComboBox.ItemsSource = FormSuppliers;
        FilterSupplierComboBox.ItemsSource = FilterSuppliers;

        StockWarehouseComboBox.ItemsSource = FormWarehouses;
        FilterWarehouseComboBox.ItemsSource = FilterWarehouses;

        StockRawMaterialComboBox.ItemsSource = RawMaterials;
        StockLotsDataGrid.ItemsSource = StockLots;
        WriteOffDataGrid.ItemsSource = StockLots;
        SuppliersDataGrid.ItemsSource = SuppliersList;

        AvailableLocationsListBox.ItemsSource = AvailableLocations;
        SelectedLocationsListBox.ItemsSource = SelectedLocations;

        ReceiptDraftDataGrid.ItemsSource = DraftItems;
        DraftItems.CollectionChanged += (s, e) =>
        {
            UpdateDraftSummary();
            ValidateCapacity();
        };

        Loaded += async (s, e) => await LoadDataAsync();
    }

    private void ApplyRolePermissions()
    {
        bool canManageSuppliers = PermissionService.Instance.HasPermission(AppPermission.SUPPLIERS_MANAGE);

        SupplierFormGroupBox.IsEnabled = canManageSuppliers;
    }

    private async Task LoadDataAsync()
    {
        try
        {
            _isDataLoaded = false;

            ApplyRolePermissions();

            bool includeInactive = (ShowInactiveSuppliersCheckBox.Visibility == Visibility.Visible)
                && (ShowInactiveSuppliersCheckBox.IsChecked ?? false);

            var suppliers = await ApiService.Instance.GetAsync<List<SupplierDto>>($"{ProcurementConstants.SUPPLIERS}?includeInactive={includeInactive}") ?? [];

            FormSuppliers.Clear();
            FilterSuppliers.Clear();
            SuppliersList.Clear();

            FilterSuppliers.Add(new LookupItem(Guid.Empty, LocalizationService.Get(UiConstants.Filters.ALL_SUPPLIERS)));

            suppliers.ForEach(s =>
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
            });

            if (FormSuppliers.Count > 0)
            {
                StockSupplierComboBox.SelectedIndex = 0;
            }

            FilterSupplierComboBox.SelectedIndex = 0;

            var warehouses = await ApiService.Instance.GetAsync<List<LookupDto>>(InventoryConstants.WAREHOUSES) ?? [];
            FormWarehouses.Clear();
            FilterWarehouses.Clear();
            FilterWarehouses.Add(new LookupItem(Guid.Empty, LocalizationService.Get(UiConstants.Filters.ALL_WAREHOUSES)));
            warehouses.ForEach(w =>
            {
                var item = new LookupItem(w.Id, w.Name);
                FormWarehouses.Add(item);
                FilterWarehouses.Add(item);
            });
            FilterWarehouseComboBox.SelectedIndex = 0;

            var raw = await ApiService.Instance.GetAsync<List<LookupDto>>($"{CatalogConstants.RAW_MATERIALS}?includeArchived=false") ?? [];
            RawMaterials.Clear();
            raw.ForEach(r => RawMaterials.Add(new LookupItem(r.Id, r.Name, r.Code)));

            _isDataLoaded = true;
            await LoadStockLotsAsync();

            SetStatus(UiConstants.Messages.DATA_LOADED_SUCCESS, Brushes.Green);
        }
        catch (Exception ex)
        {
            SetStatusFormatted(UiConstants.Messages.DATA_LOAD_ERROR, Brushes.Red, ex.Message);
            AlertWindow.ShowError(Window.GetWindow(this), ex.Message);
        }
    }

    private async void SaveSupplier_Click(object sender, RoutedEventArgs e)
    {
        if (!PermissionService.Instance.HasPermission(AppPermission.SUPPLIERS_MANAGE))
        {
            AlertWindow.ShowError(Window.GetWindow(this), LocalizationService.Get(UI_SUPPLIERS_MANAGE_REQUIRED));
            return;
        }

        var name = NameTextBox.Text.Trim();
        var legalName = string.IsNullOrWhiteSpace(LegalNameTextBox.Text) ? null : LegalNameTextBox.Text.Trim();
        var taxId = string.IsNullOrWhiteSpace(TaxIdTextBox.Text) ? null : TaxIdTextBox.Text.Trim();
        var phone = string.IsNullOrWhiteSpace(PhoneTextBox.Text) ? null : PhoneTextBox.Text.Trim();
        var email = string.IsNullOrWhiteSpace(EmailTextBox.Text) ? null : EmailTextBox.Text.Trim();
        var address = string.IsNullOrWhiteSpace(AddressTextBox.Text) ? null : AddressTextBox.Text.Trim();
        var notes = string.IsNullOrWhiteSpace(NotesTextBox.Text) ? null : NotesTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
            AlertWindow.ShowError(Window.GetWindow(this), LocalizationService.Get(UiConstants.Messages.INVALID_INPUT_FIELDS));
            return;
        }

        bool isSuccess;
        string responseStr;

        if (_editingSupplierId.HasValue)
        {
            var updateRequest = new UpdateSupplierRequest(name, legalName, taxId, phone, email, address, notes);
            (isSuccess, responseStr) = await ApiService.Instance.PutAndReadAsync($"{ProcurementConstants.SUPPLIERS}/{_editingSupplierId.Value}", updateRequest);
        }
        else
        {
            var createRequest = new CreateSupplierRequest(name, legalName, taxId, phone, email, address, notes);
            (isSuccess, responseStr) = await ApiService.Instance.PostAndReadAsync(ProcurementConstants.SUPPLIERS, createRequest);
        }

        if (isSuccess)
        {
            SetStatus(UiConstants.Messages.SUPPLIER_CREATED_SUCCESS, Brushes.Green);
            AlertWindow.ShowSuccess(Window.GetWindow(this), _editingSupplierId.HasValue ? LocalizationService.Get(UI_SUPPLIER_UPDATED_SUCCESS) : LocalizationService.Get(UiConstants.Messages.SUPPLIER_CREATED_SUCCESS));

            ResetSupplierForm();
            await LoadDataAsync();
        }
        else
        {
            SetStatusRaw(responseStr, Brushes.Red);
            AlertWindow.ShowError(Window.GetWindow(this), responseStr);
        }
    }

    private void EditSupplierRow_Click(object sender, RoutedEventArgs e)
    {
        if (!PermissionService.Instance.HasPermission(AppPermission.SUPPLIERS_MANAGE))
            return;

        if (sender is Button btn && btn.Tag is SupplierGridDto supplier)
        {
            PopulateFormForEdit(supplier);
        }
    }

    private void SuppliersDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (!PermissionService.Instance.HasPermission(AppPermission.SUPPLIERS_MANAGE))
            return;

        if (SuppliersDataGrid.SelectedItem is SupplierGridDto supplier)
        {
            PopulateFormForEdit(supplier);
        }
    }

    private void PopulateFormForEdit(SupplierGridDto supplier)
    {
        _editingSupplierId = supplier.Id;
        NameTextBox.Text = supplier.Name;
        LegalNameTextBox.Text = supplier.LegalName;
        TaxIdTextBox.Text = supplier.TaxId;
        PhoneTextBox.Text = supplier.Phone;
        EmailTextBox.Text = supplier.Email;
        AddressTextBox.Text = supplier.Address;
        NotesTextBox.Text = supplier.Notes;

        SupplierFormGroupBox.Header = LocalizationService.Get(UI_HEADER_SUPPLIER_EDIT);
        SaveSupplierButton.Content = LocalizationService.Get(UI_BTN_SAVE_CHANGES);
        ResetFormButton.Visibility = Visibility.Visible;
    }

    private void ResetSupplierForm_Click(object sender, RoutedEventArgs e) => ResetSupplierForm();

    private void ResetSupplierForm()
    {
        _editingSupplierId = null;
        NameTextBox.Clear();
        LegalNameTextBox.Clear();
        TaxIdTextBox.Clear();
        PhoneTextBox.Clear();
        EmailTextBox.Clear();
        AddressTextBox.Clear();
        NotesTextBox.Clear();

        SupplierFormGroupBox.Header = LocalizationService.Get("HEADER_SUPPLIERS");
        SaveSupplierButton.Content = LocalizationService.Get("CREATE_SUPPLIER");
        ResetFormButton.Visibility = Visibility.Collapsed;
    }

    private async void ToggleSupplierActive_Click(object sender, RoutedEventArgs e)
    {
        if (!PermissionService.Instance.HasPermission(AppPermission.SUPPLIERS_MANAGE))
            return;

        if (sender is Button btn && btn.Tag is SupplierGridDto supplier)
        {
            var (isSuccess, responseStr) = await ApiService.Instance.PutAndReadAsync($"{ProcurementConstants.SUPPLIERS}/{supplier.Id}/toggle-active", new { });
            if (isSuccess)
            {
                await LoadDataAsync();
            }
            else
            {
                AlertWindow.ShowError(Window.GetWindow(this), responseStr);
            }
        }
    }

    private async void ShowInactiveSuppliers_Click(object sender, RoutedEventArgs e)
    {
        await LoadDataAsync();
    }

    private void StockRawMaterialComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        AutoSelectUnitOfMeasure();
        ValidateCapacity();
    }

    private void AutoSelectUnitOfMeasure()
    {
        if (StockRawMaterialComboBox.SelectedItem is LookupItem selectedRaw && !string.IsNullOrWhiteSpace(selectedRaw.Code))
        {
            SelectUnitInComboBox(selectedRaw.Code);
        }
    }

    private void SelectUnitInComboBox(string unitTag)
    {
        foreach (ComboBoxItem item in UnitOfMeasureComboBox.Items)
        {
            if (string.Equals(item.Tag?.ToString(), unitTag, StringComparison.OrdinalIgnoreCase))
            {
                UnitOfMeasureComboBox.SelectedItem = item;
                break;
            }
        }
    }

    private string GetSelectedUnitTag()
    {
        if (UnitOfMeasureComboBox.SelectedItem is ComboBoxItem item && item.Tag != null)
        {
            return item.Tag.ToString() ?? "l";
        }
        return "l";
    }

    private string GetSelectedUnitDisplayText()
    {
        if (UnitOfMeasureComboBox.SelectedItem is ComboBoxItem item)
        {
            return item.Content?.ToString() ?? "л";
        }
        return "л";
    }

    private void UnitOfMeasureComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ValidateCapacity();
    }

    private async void StockWarehouseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SelectedLocations.Clear();
        DraftItems.Clear();

        if (StockWarehouseComboBox.SelectedValue is Guid warehouseId && warehouseId != Guid.Empty)
        {
            await LoadLocationsForWarehouseAsync(warehouseId);
        }
        else
        {
            AvailableLocations.Clear();
        }
        ValidateCapacity();
    }

    private async Task LoadLocationsForWarehouseAsync(Guid warehouseId)
    {
        try
        {
            var locations = await ApiService.Instance.GetAsync<List<StorageLocationDto>>($"{InventoryConstants.WAREHOUSES_LOCATIONS}?warehouseId={warehouseId}");
            AvailableLocations.Clear();

            locations?.Where(l => l.LocationType != StorageLocationType.BREWING_VESSEL
                               && l.Status == StorageLocationStatus.AVAILABLE)
                     .ToList()
                     .ForEach(l => AvailableLocations.Add(l));
        }
        catch { }
    }

    private void AddLocationToSelected_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is StorageLocationDto location)
        {
            AvailableLocations.Remove(location);
            SelectedLocations.Add(location);

            if (!string.IsNullOrWhiteSpace(location.UnitOfMeasure))
            {
                SelectUnitInComboBox(location.UnitOfMeasure);
            }

            ValidateCapacity();
        }
    }

    private void RemoveLocationFromSelected_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is StorageLocationDto location)
        {
            SelectedLocations.Remove(location);
            AvailableLocations.Add(location);
            ValidateCapacity();
        }
    }

    private void StockQuantityTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ValidateCapacity();
    }

    private void ValidateCapacity()
    {
        if (CapacityWarningTextBlock == null || AddDraftLineButton == null || StockQuantityTextBox == null)
            return;

        if (string.IsNullOrWhiteSpace(StockQuantityTextBox.Text))
        {
            CapacityWarningTextBlock.Text = string.Empty;
            AddDraftLineButton.IsEnabled = true;
            return;
        }

        var rawQuantityText = StockQuantityTextBox.Text.Replace(',', '.');
        if (!decimal.TryParse(rawQuantityText, NumberStyles.Any, CultureInfo.InvariantCulture, out var requiredQuantity) || requiredQuantity <= 0)
        {
            CapacityWarningTextBlock.Text = LocalizationService.Get(UiConstants.Messages.INVALID_INPUT_FIELDS);
            CapacityWarningTextBlock.Foreground = Brushes.Red;
            AddDraftLineButton.IsEnabled = false;
            return;
        }

        if (SelectedLocations.Count == 0)
        {
            CapacityWarningTextBlock.Text = string.Empty;
            AddDraftLineButton.IsEnabled = true;
            return;
        }

        // Учитываем объём, уже зарезервированный в текущем черновике накладной под выбранные ёмкости
        var selectedIds = SelectedLocations.Select(l => l.Id).ToHashSet();
        decimal reservedInDraft = DraftItems
            .Where(d => d.StorageLocationIds.Any(id => selectedIds.Contains(id)))
            .Sum(d => d.Quantity);

        decimal totalCapacity = 0;
        foreach (var loc in SelectedLocations)
        {
            decimal cap = loc.Capacity ?? 0;
            decimal freeCap = cap > loc.CurrentVolume ? cap - loc.CurrentVolume : 0;
            totalCapacity += freeCap;
        }

        decimal effectiveFreeCapacity = Math.Max(0, totalCapacity - reservedInDraft);

        if (effectiveFreeCapacity < requiredQuantity)
        {
            var format = LocalizationService.Get("CAPACITY_WARNING_INSUFFICIENT");
            CapacityWarningTextBlock.Text = string.Format(format, requiredQuantity, effectiveFreeCapacity);
            CapacityWarningTextBlock.Foreground = Brushes.Red;
            AddDraftLineButton.IsEnabled = false;
        }
        else
        {
            var format = LocalizationService.Get("CAPACITY_WARNING_SUFFICIENT");
            CapacityWarningTextBlock.Text = string.Format(format, effectiveFreeCapacity, requiredQuantity);
            CapacityWarningTextBlock.Foreground = Brushes.Green;
            AddDraftLineButton.IsEnabled = true;
        }
    }

    private void DraftInput_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            AddDraftLine_Click(sender, e);
        }
    }

    private void AddDraftLine_Click(object sender, RoutedEventArgs e)
    {
        if (StockRawMaterialComboBox.SelectedItem is not LookupItem selectedRawMaterial)
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
            return;
        }

        var rawQuantityText = StockQuantityTextBox.Text.Replace(',', '.');
        var rawPriceText = StockUnitPriceTextBox.Text.Replace(',', '.');

        if (!decimal.TryParse(rawQuantityText, NumberStyles.Any, CultureInfo.InvariantCulture, out var quantity) || quantity <= 0 ||
            !decimal.TryParse(rawPriceText, NumberStyles.Any, CultureInfo.InvariantCulture, out var unitPrice) || unitPrice < 0)
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
            return;
        }

        var batchNumber = string.IsNullOrWhiteSpace(StockBatchTextBox.Text)
            ? InvoiceNumberTextBox.Text.Trim()
            : StockBatchTextBox.Text.Trim();

        var locationIds = SelectedLocations.Select(l => l.Id).ToList();
        var locationsDisplay = SelectedLocations.Count > 0
            ? string.Join(", ", SelectedLocations.Select(l => l.Name))
            : "—";

        DraftItems.Add(new ReceiptDraftItem
        {
            ItemId = selectedRawMaterial.Id,
            ItemName = selectedRawMaterial.Name,
            Quantity = quantity,
            UnitPrice = unitPrice,
            UnitOfMeasure = GetSelectedUnitTag(),
            UnitOfMeasureDisplay = GetSelectedUnitDisplayText(),
            BatchNumber = batchNumber,
            ExpirationDate = ExpirationDatePicker.SelectedDate,
            StorageLocationIds = locationIds,
            StorageLocationsDisplay = locationsDisplay
        });

        // Возвращаем выбранные ёмкости обратно в доступные для следующей позиции накладной
        foreach (var loc in SelectedLocations.ToList())
        {
            SelectedLocations.Remove(loc);
            AvailableLocations.Add(loc);
        }

        StockRawMaterialComboBox.SelectedItem = null;
        StockRawMaterialComboBox.Text = string.Empty;
        StockQuantityTextBox.Clear();
        StockUnitPriceTextBox.Clear();
        StockBatchTextBox.Clear();
        ExpirationDatePicker.SelectedDate = null;
        CapacityWarningTextBlock.Text = string.Empty;

        StockRawMaterialComboBox.Focus();
    }

    private void RemoveDraftLine_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is ReceiptDraftItem item)
        {
            DraftItems.Remove(item);
        }
    }

    private void ClearDraftLines_Click(object sender, RoutedEventArgs e)
    {
        DraftItems.Clear();
    }

    private void ReceiptDraftDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        Dispatcher.BeginInvoke(() =>
        {
            UpdateDraftSummary();
            ValidateCapacity();
        }, DispatcherPriority.Background);
    }

    private void UpdateDraftSummary()
    {
        int count = DraftItems.Count;
        decimal total = DraftItems.Sum(x => x.LineTotal);

        DraftSummaryTextBlock.Text = $"Позиций в накладной: {count} | Итого: ${total:F2}";
        SubmitReceiptBatchButton.IsEnabled = count > 0;
    }

    private void ImportInvoiceFile_Click(object sender, RoutedEventArgs e)
    {
        AlertWindow.ShowSuccess(Window.GetWindow(this), "Модуль автозагрузки накладных из Excel / PDF в разработке. Распознанные позиции будут загружаться в эту таблицу для проверки перед проведением.");
    }

    private async void SubmitReceiptBatch_Click(object sender, RoutedEventArgs e)
    {
        if (DraftItems.Count == 0) return;

        if (StockSupplierComboBox.SelectedItem is not LookupItem selectedSupplier ||
            StockWarehouseComboBox.SelectedItem is not LookupItem selectedWarehouse)
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
            AlertWindow.ShowError(Window.GetWindow(this), LocalizationService.Get(UiConstants.Messages.INVALID_INPUT_FIELDS));
            return;
        }

        var request = new ReceiveStockBatchRequest(
            SupplierId: selectedSupplier.Id,
            WarehouseId: selectedWarehouse.Id,
            InvoiceNumber: string.IsNullOrWhiteSpace(InvoiceNumberTextBox.Text) ? null : InvoiceNumberTextBox.Text.Trim(),
            Items: DraftItems.Select(x => new StockReceiptLineDto(
                ItemId: x.ItemId,
                Quantity: x.Quantity,
                UnitPrice: x.UnitPrice,
                UnitOfMeasureId: Guid.Empty,
                BatchNumber: string.IsNullOrWhiteSpace(x.BatchNumber) ? null : x.BatchNumber,
                ExpirationDate: x.ExpirationDate,
                StorageLocationIds: x.StorageLocationIds.Count > 0 ? x.StorageLocationIds : null
            )).ToList()
        );

        SubmitReceiptBatchButton.IsEnabled = false;
        try
        {
            var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(InventoryConstants.STOCK_LOTS_BATCH, request);

            if (isSuccess)
            {
                DraftItems.Clear();
                SetStatus(UiConstants.Messages.STOCK_LOT_CREATED_SUCCESS, Brushes.Green);
                AlertWindow.ShowSuccess(Window.GetWindow(this), LocalizationService.Get(UiConstants.Messages.STOCK_LOT_CREATED_SUCCESS));

                await LoadStockLotsAsync();
                if (selectedWarehouse.Id != Guid.Empty)
                {
                    await LoadLocationsForWarehouseAsync(selectedWarehouse.Id);
                }
            }
            else
            {
                SetStatusRaw(contentOrError, Brushes.Red);
                AlertWindow.ShowError(Window.GetWindow(this), contentOrError);
            }
        }
        finally
        {
            UpdateDraftSummary();
        }
    }

    private async Task LoadStockLotsAsync()
    {
        if (!_isDataLoaded) return;

        try
        {
            Guid? selectedSupplierId = FilterSupplierComboBox.SelectedValue is Guid supId && supId != Guid.Empty ? supId : null;
            Guid? selectedWarehouseId = FilterWarehouseComboBox.SelectedValue is Guid whId && whId != Guid.Empty ? whId : null;
            bool onlyExpiringSoon = FilterExpiringCheckBox.IsChecked ?? false;

            var queryParams = new List<string> { $"onlyExpiringSoon={onlyExpiringSoon}" };
            if (selectedSupplierId.HasValue) queryParams.Add($"supplierId={selectedSupplierId.Value}");
            if (selectedWarehouseId.HasValue) queryParams.Add($"warehouseId={selectedWarehouseId.Value}");

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

    private async void FilterStockLots_Changed(object sender, RoutedEventArgs e) => await LoadStockLotsAsync();
    private async void RefreshStockLots_Click(object sender, RoutedEventArgs e) => await LoadStockLotsAsync();

    private async void WriteOffRow_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Guid lotId)
        {
            var lot = StockLots.FirstOrDefault(l => l.Id == lotId);
            if (lot != null)
            {
                if (lot.Quantity <= 0m)
                {
                    SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
                    AlertWindow.ShowError(Window.GetWindow(this), LocalizationService.Get(UiConstants.Messages.INVALID_INPUT_FIELDS));
                    return;
                }

                List<GetAgingLotItemDto>? agingItems = await ApiService.Instance.GetAsync<List<GetAgingLotItemDto>>($"{InventoryConstants.STOCK_LOTS}/{lot.Id}/items");

                var dialog = new WriteOffDialog(lot.ItemName, lot.BatchNumber, lot.Quantity, items: agingItems)
                {
                    Owner = Window.GetWindow(this)
                };

                if (dialog.ShowDialog() == true)
                {
                    await ExecuteWriteOffAsync(lot.Id, dialog.QuantityToWriteOff, dialog.SelectedItemIds, dialog.Reason);
                }
            }
        }
    }

    private async void QuickCreateLocation_Click(object sender, RoutedEventArgs e)
    {
        Guid? currentWarehouseId = StockWarehouseComboBox.SelectedValue as Guid?;

        var dialog = new CreateStorageLocationWindow(currentWarehouseId)
        {
            Owner = Window.GetWindow(this)
        };

        if (dialog.ShowDialog() == true && currentWarehouseId.HasValue && currentWarehouseId != Guid.Empty)
        {
            await LoadLocationsForWarehouseAsync(currentWarehouseId.Value);
        }
    }

    private async Task ExecuteWriteOffAsync(Guid stockLotId, decimal quantity, List<Guid>? itemIds = null, string? reason = null)
    {
        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(InventoryConstants.STOCK_LOTS_WRITE_OFF, new
        {
            StockLotId = stockLotId,
            QuantityToWriteOff = quantity,
            SelectedItemIds = itemIds,
            Reason = reason
        });

        if (isSuccess)
        {
            SetStatus(UiConstants.Messages.DATA_LOADED_SUCCESS, Brushes.Green);
            AlertWindow.ShowSuccess(Window.GetWindow(this), LocalizationService.Get(UiConstants.Messages.DATA_LOADED_SUCCESS));
            await LoadStockLotsAsync();
        }
        else
        {
            SetStatusRaw(contentOrError, Brushes.Red);
            AlertWindow.ShowError(Window.GetWindow(this), contentOrError);
        }
    }

    private void SetStatus(string resourceKey, Brush color)
    {
        StatusTextBlock.Foreground = color;
        StatusTextBlock.Text = LocalizationService.Get(resourceKey);
    }

    private void SetStatusFormatted(string resourceKey, Brush color, params object[] args)
    {
        StatusTextBlock.Foreground = color;
        var format = LocalizationService.Get(resourceKey);
        StatusTextBlock.Text = string.Format(format, args);
    }

    private void SetStatusRaw(string rawText, Brush color)
    {
        StatusTextBlock.Foreground = color;
        StatusTextBlock.Text = rawText;
    }
}