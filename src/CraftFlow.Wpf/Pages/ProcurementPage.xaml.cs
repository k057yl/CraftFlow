using CraftFlow.Api.Modules.Catalog;
using CraftFlow.Api.Modules.Inventory;
using CraftFlow.Api.Modules.Procurement;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Dtos.Aging;
using CraftFlow.SharedKernel.Dtos.Common;
using CraftFlow.SharedKernel.Dtos.Inventory;
using CraftFlow.SharedKernel.Dtos.Supplier;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Enums.Inventory;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Services;
using CraftFlow.Wpf.Windows;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CraftFlow.Wpf.Pages;

public partial class ProcurementPage : Page
{
    public ObservableCollection<LookupItem> FormSuppliers { get; } = [];
    public ObservableCollection<LookupItem> FilterSuppliers { get; } = [];

    public ObservableCollection<LookupItem> FormWarehouses { get; } = [];
    public ObservableCollection<LookupItem> FilterWarehouses { get; } = [];

    public ObservableCollection<LookupItem> RawMaterials { get; } = [];
    public ObservableCollection<StockLotGridDto> StockLots { get; } = [];
    public ObservableCollection<SupplierGridDto> SuppliersList { get; } = [];

    public ObservableCollection<StorageLocationDto> AvailableLocations { get; } = [];
    public ObservableCollection<StorageLocationDto> SelectedLocations { get; } = [];

    private bool _isDataLoaded = false;
    private Guid? _editingSupplierId = null;

    public ProcurementPage()
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

        Loaded += async (s, e) => await LoadDataAsync();
    }

    private void ApplyRolePermissions()
    {
        var currentUser = ApiService.Instance.CurrentUser;
        bool hasManagementAccess = currentUser != null &&
            (currentUser.Role == TenantRole.OWNER || currentUser.Role == TenantRole.ACCOUNTANT);

        SuppliersTabItem.Visibility = hasManagementAccess ? Visibility.Visible : Visibility.Collapsed;

        SupplierActionsColumn.Visibility = hasManagementAccess ? Visibility.Visible : Visibility.Collapsed;
        ShowInactiveSuppliersCheckBox.Visibility = hasManagementAccess ? Visibility.Visible : Visibility.Collapsed;

        SupplierFormGroupBox.IsEnabled = hasManagementAccess;
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
        var currentUser = ApiService.Instance.CurrentUser;
        if (currentUser == null || (currentUser.Role != TenantRole.OWNER && currentUser.Role != TenantRole.ACCOUNTANT))
        {
            AlertWindow.ShowError(Window.GetWindow(this), "Недостаточно прав для выполнения операции.");
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
            AlertWindow.ShowError(Window.GetWindow(this), UiConstants.Messages.INVALID_INPUT_FIELDS);
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
            AlertWindow.ShowSuccess(Window.GetWindow(this), _editingSupplierId.HasValue ? "Данные поставщика обновлены" : UiConstants.Messages.SUPPLIER_CREATED_SUCCESS);

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
        var currentUser = ApiService.Instance.CurrentUser;
        if (currentUser == null || (currentUser.Role != TenantRole.OWNER && currentUser.Role != TenantRole.ACCOUNTANT))
            return;

        if (sender is Button btn && btn.Tag is SupplierGridDto supplier)
        {
            PopulateFormForEdit(supplier);
        }
    }

    private void SuppliersDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        var currentUser = ApiService.Instance.CurrentUser;
        if (currentUser == null || (currentUser.Role != TenantRole.OWNER && currentUser.Role != TenantRole.ACCOUNTANT))
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

        SupplierFormGroupBox.Header = "Редактирование поставщика";
        SaveSupplierButton.Content = "Сохранить изменения";
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
            var locations = await ApiService.Instance.GetAsync<List<StorageLocationDto>>($"{InventoryConstants.WAREHOUSES}/locations?warehouseId={warehouseId}");
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
        if (CapacityWarningTextBlock == null || AddStockLotButton == null || StockQuantityTextBox == null)
        {
            return;
        }

        var rawQuantityText = StockQuantityTextBox.Text.Replace(',', '.');
        if (!decimal.TryParse(rawQuantityText, NumberStyles.Any, CultureInfo.InvariantCulture, out var requiredQuantity) || requiredQuantity <= 0)
        {
            CapacityWarningTextBlock.Text = LocalizationService.Get(UiConstants.Messages.INVALID_INPUT_FIELDS);
            CapacityWarningTextBlock.Foreground = Brushes.Red;
            AddStockLotButton.IsEnabled = false;
            return;
        }

        if (SelectedLocations.Count == 0)
        {
            CapacityWarningTextBlock.Text = string.Empty;
            AddStockLotButton.IsEnabled = true;
            return;
        }

        string unitDisplay = GetSelectedUnitDisplayText();

        decimal totalCapacity = 0;
        foreach (var loc in SelectedLocations)
        {
            decimal cap = loc.Capacity ?? 0;
            decimal freeCap = cap > loc.CurrentVolume ? cap - loc.CurrentVolume : 0;
            totalCapacity += freeCap;
        }

        if (totalCapacity < requiredQuantity)
        {
            CapacityWarningTextBlock.Text = $"Недостаточно емкости: выбрано на {totalCapacity} {unitDisplay} из необходимых {requiredQuantity} {unitDisplay}";
            CapacityWarningTextBlock.Foreground = Brushes.Red;
            AddStockLotButton.IsEnabled = false;
        }
        else
        {
            CapacityWarningTextBlock.Text = $"Вместимость подходит (выбрано на {totalCapacity} {unitDisplay} под партию в {requiredQuantity} {unitDisplay})";
            CapacityWarningTextBlock.Foreground = Brushes.Green;
            AddStockLotButton.IsEnabled = true;
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

    private async void AddStockLot_Click(object sender, RoutedEventArgs e)
    {
        if (StockSupplierComboBox.SelectedItem is not LookupItem selectedSupplier ||
            StockWarehouseComboBox.SelectedItem is not LookupItem selectedWarehouse ||
            StockRawMaterialComboBox.SelectedItem is not LookupItem selectedRawMaterial)
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
            AlertWindow.ShowError(Window.GetWindow(this), UiConstants.Messages.INVALID_INPUT_FIELDS);
            return;
        }

        var rawQuantityText = StockQuantityTextBox.Text.Replace(',', '.');
        var rawPriceText = StockUnitPriceTextBox.Text.Replace(',', '.');

        if (!decimal.TryParse(rawQuantityText, NumberStyles.Any, CultureInfo.InvariantCulture, out var quantity) ||
            !decimal.TryParse(rawPriceText, NumberStyles.Any, CultureInfo.InvariantCulture, out var unitPrice))
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
            AlertWindow.ShowError(Window.GetWindow(this), UiConstants.Messages.INVALID_INPUT_FIELDS);
            return;
        }

        int? unitsCount = null;
        DateTime? expirationDate = ExpirationDatePicker.SelectedDate;
        var selectedLocationIds = SelectedLocations.Select(l => l.Id).ToList();
        var unitOfMeasureTag = GetSelectedUnitTag();

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(InventoryConstants.STOCK_LOTS, new
        {
            SupplierId = selectedSupplier.Id,
            WarehouseId = selectedWarehouse.Id,
            ItemId = selectedRawMaterial.Id,
            Quantity = quantity,
            UnitsCount = unitsCount,
            UnitPrice = unitPrice,
            UnitOfMeasure = unitOfMeasureTag,
            BatchNumber = StockBatchTextBox.Text,
            ExpirationDate = expirationDate,
            StorageLocationIds = selectedLocationIds.Count > 0 ? selectedLocationIds : null
        });

        if (isSuccess)
        {
            SetStatus(UiConstants.Messages.STOCK_LOT_CREATED_SUCCESS, Brushes.Green);
            AlertWindow.ShowSuccess(Window.GetWindow(this), UiConstants.Messages.STOCK_LOT_CREATED_SUCCESS);
            ExpirationDatePicker.SelectedDate = null;
            SelectedLocations.Clear();
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
                    AlertWindow.ShowError(Window.GetWindow(this), UiConstants.Messages.INVALID_INPUT_FIELDS);
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
        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync($"{InventoryConstants.STOCK_LOTS}/write-off", new
        {
            StockLotId = stockLotId,
            QuantityToWriteOff = quantity,
            SelectedItemIds = itemIds,
            Reason = reason
        });

        if (isSuccess)
        {
            SetStatus(UiConstants.Messages.DATA_LOADED_SUCCESS, Brushes.Green);
            AlertWindow.ShowSuccess(Window.GetWindow(this), UiConstants.Messages.DATA_LOADED_SUCCESS);
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