using CraftFlow.Api.Modules.Aging;
using CraftFlow.Api.Modules.Inventory;
using CraftFlow.SharedKernel.Dtos.Common;
using CraftFlow.SharedKernel.Dtos.Inventory;
using CraftFlow.SharedKernel.Enums.Inventory;
using CraftFlow.SharedKernel.Enums.Sanitation;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Services;
using CraftFlow.Wpf.Windows;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static CraftFlow.Wpf.Services.LocalizationService;

namespace CraftFlow.Wpf.Pages.ProductionSettings.Controls;

public partial class StorageControl : UserControl
{
    public event Action<string, Brush>? StatusUpdated;

    private List<StorageLocationViewItem> _allStorageLocations = [];

    public ObservableCollection<LookupItem> Warehouses { get; } = [];
    public ObservableCollection<LookupItem> Chambers { get; } = [];
    public ObservableCollection<StorageLocationViewItem> StorageLocations { get; } = [];

    public StorageControl()
    {
        InitializeComponent();

        WarehousesDataGrid.ItemsSource = Warehouses;
        ChambersDataGrid.ItemsSource = Chambers;
        StorageLocationsDataGrid.ItemsSource = StorageLocations;

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

    private async Task LoadDataAsync()
    {
        try
        {
            var warehouses = await ApiService.Instance.GetAsync<List<LookupDto>>(InventoryConstants.WAREHOUSES);
            Warehouses.Clear();
            warehouses?.ForEach(w => Warehouses.Add(new LookupItem(w.Id, w.Name)));

            var chambers = await ApiService.Instance.GetAsync<List<LookupDto>>(AgingConstants.AGING_CHAMBERS);
            Chambers.Clear();
            chambers?.ForEach(c => Chambers.Add(new LookupItem(c.Id, c.Name)));

            var locations = await ApiService.Instance.GetAsync<List<StorageLocationDto>>($"{InventoryConstants.WAREHOUSES}/locations");
            _allStorageLocations.Clear();

            var now = DateTime.UtcNow;

            locations?.ForEach(l =>
            {
                string parentName = !string.IsNullOrWhiteSpace(l.WarehouseName)
                    ? $"🏠 {l.WarehouseName}"
                    : (!string.IsNullOrWhiteSpace(l.ChamberName) ? $"❄ {l.ChamberName}" : "—");

                string statusText = Get("UI_STORAGE_STATUS_OK");
                Brush statusBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E7D32"));
                Brush backgroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8F5E9"));
                string code = "OK";

                if (l.NextWashDueDate.HasValue)
                {
                    if (l.NextWashDueDate.Value < now)
                    {
                        statusText = Get("UI_STORAGE_STATUS_OVERDUE");
                        statusBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C62828"));
                        backgroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEBEE"));
                        code = "OVERDUE";
                    }
                    else if ((l.NextWashDueDate.Value - now).TotalDays <= 2)
                    {
                        statusText = Get("UI_STORAGE_STATUS_WARNING");
                        statusBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF6C00"));
                        backgroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3E0"));
                        code = "WARNING";
                    }
                }

                _allStorageLocations.Add(new StorageLocationViewItem
                {
                    Id = l.Id,
                    Name = l.Name,
                    LocationType = l.LocationType,
                    VolumeInfo = l.VolumeInfo,
                    ParentName = parentName,
                    LastWashedText = l.LastWashedAt?.ToString("dd.MM.yyyy HH:mm") ?? "—",
                    NextWashText = l.NextWashDueDate?.ToString("dd.MM.yyyy") ?? "—",
                    SanitationStatusCode = code,
                    SanitationStatusText = statusText,
                    StatusForeground = statusBrush,
                    StatusBackground = backgroundBrush
                });
            });

            ApplyFilters();
            UpdateSanitationCounters();

            SetStatus("UI_MSG_DATA_LOADED_SUCCESS", Brushes.Green);
        }
        catch (Exception ex)
        {
            SetStatusFormatted("UI_ERR_DATA_LOAD_FMT", Brushes.Red, ex.Message);
        }
    }

    #region Выпадающие меню

    private void OpenWarehouseMenu_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is LookupItem item)
        {
            var menu = new ContextMenu();

            var planItem = new MenuItem { Header = Get("UI_STORAGE_MENU_SANITATION_PLAN") };
            planItem.Click += (s, args) => OpenWarehouseSanitation(item);

            var detailsItem = new MenuItem { Header = Get("UI_STORAGE_MENU_SANITATION_DETAILS") };
            detailsItem.Click += (s, args) => OpenWarehouseDetails(item);

            var deleteItem = new MenuItem { Header = Get("UI_STORAGE_MENU_DELETE") };
            deleteItem.Click += async (s, args) => await DeleteWarehouseAsync(item.Id);

            menu.Items.Add(planItem);
            menu.Items.Add(detailsItem);
            menu.Items.Add(new Separator());
            menu.Items.Add(deleteItem);

            menu.PlacementTarget = btn;
            menu.IsOpen = true;
        }
    }

    private void OpenChamberMenu_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is LookupItem item)
        {
            var menu = new ContextMenu();

            var planItem = new MenuItem { Header = Get("UI_STORAGE_MENU_SANITATION_PLAN") };
            planItem.Click += (s, args) => OpenChamberSanitation(item);

            var detailsItem = new MenuItem { Header = Get("UI_STORAGE_MENU_SANITATION_DETAILS") };
            detailsItem.Click += (s, args) => OpenChamberDetails(item);

            var deleteItem = new MenuItem { Header = Get("UI_STORAGE_MENU_DELETE") };
            deleteItem.Click += async (s, args) => await DeleteChamberAsync(item.Id);

            menu.Items.Add(planItem);
            menu.Items.Add(detailsItem);
            menu.Items.Add(new Separator());
            menu.Items.Add(deleteItem);

            menu.PlacementTarget = btn;
            menu.IsOpen = true;
        }
    }

    private void OpenLocationMenu_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is StorageLocationViewItem item)
        {
            var menu = new ContextMenu();

            var planItem = new MenuItem { Header = Get("UI_STORAGE_MENU_SANITATION_PLAN") };
            planItem.Click += (s, args) => OpenLocationSanitation(item);

            var detailsItem = new MenuItem { Header = Get("UI_STORAGE_MENU_SANITATION_DETAILS") };
            detailsItem.Click += (s, args) => OpenLocationDetails(item);

            var deleteItem = new MenuItem { Header = Get("UI_STORAGE_MENU_DELETE") };
            deleteItem.Click += async (s, args) => await DeleteStorageLocationAsync(item.Id);

            menu.Items.Add(planItem);
            menu.Items.Add(detailsItem);
            menu.Items.Add(new Separator());
            menu.Items.Add(deleteItem);

            menu.PlacementTarget = btn;
            menu.IsOpen = true;
        }
    }

    #endregion

    #region Логика диалогов

    private async void OpenWarehouseSanitation(LookupItem item)
    {
        var dialog = new SanitationRuleWindow(item.Id, item.Name, SanitationTargetType.WAREHOUSE)
        {
            Owner = Window.GetWindow(this)
        };
        if (dialog.ShowDialog() == true) await LoadDataAsync();
    }

    private async void OpenWarehouseDetails(LookupItem item)
    {
        var detailsWindow = new SanitationDetailsWindow(item.Id, item.Name, SanitationTargetType.WAREHOUSE)
        {
            Owner = Window.GetWindow(this)
        };
        detailsWindow.ShowDialog();
        await LoadDataAsync();
    }

    private async void OpenChamberSanitation(LookupItem item)
    {
        var dialog = new SanitationRuleWindow(item.Id, item.Name, SanitationTargetType.AGING_CHAMBER)
        {
            Owner = Window.GetWindow(this)
        };
        if (dialog.ShowDialog() == true) await LoadDataAsync();
    }

    private async void OpenChamberDetails(LookupItem item)
    {
        var detailsWindow = new SanitationDetailsWindow(item.Id, item.Name, SanitationTargetType.AGING_CHAMBER)
        {
            Owner = Window.GetWindow(this)
        };
        detailsWindow.ShowDialog();
        await LoadDataAsync();
    }

    private async void OpenLocationSanitation(StorageLocationViewItem item)
    {
        var dialog = new SanitationRuleWindow(item.Id, item.Name, SanitationTargetType.STORAGE_LOCATION)
        {
            Owner = Window.GetWindow(this)
        };
        if (dialog.ShowDialog() == true) await LoadDataAsync();
    }

    private async void OpenLocationDetails(StorageLocationViewItem item)
    {
        var detailsWindow = new SanitationDetailsWindow(item.Id, item.Name, SanitationTargetType.STORAGE_LOCATION)
        {
            Owner = Window.GetWindow(this)
        };
        detailsWindow.ShowDialog();
        await LoadDataAsync();
    }

    #endregion

    #region Удаление

    private async Task DeleteWarehouseAsync(Guid id)
    {
        var owner = Window.GetWindow(this);
        if (!ConfirmWindow.Show(owner, "UI_STORAGE_CONFIRM_DELETE_WAREHOUSE")) return;

        var (isSuccess, error) = await ApiService.Instance.DeleteAndReadAsync($"{InventoryConstants.WAREHOUSES}/{id}");
        if (isSuccess)
        {
            SetStatus("UI_MSG_DATA_LOADED_SUCCESS", Brushes.Green);
            await LoadDataAsync();
        }
        else
        {
            AlertWindow.ShowError(owner, error);
        }
    }

    private async Task DeleteChamberAsync(Guid id)
    {
        var owner = Window.GetWindow(this);
        if (!ConfirmWindow.Show(owner, "UI_STORAGE_CONFIRM_DELETE_CHAMBER")) return;

        var (isSuccess, error) = await ApiService.Instance.DeleteAndReadAsync($"{AgingConstants.AGING_CHAMBERS}/{id}");
        if (isSuccess)
        {
            SetStatus("UI_MSG_DATA_LOADED_SUCCESS", Brushes.Green);
            await LoadDataAsync();
        }
        else
        {
            AlertWindow.ShowError(owner, error);
        }
    }

    private async Task DeleteStorageLocationAsync(Guid id)
    {
        var owner = Window.GetWindow(this);
        if (!ConfirmWindow.Show(owner, "UI_STORAGE_CONFIRM_DELETE_LOCATION")) return;

        var (isSuccess, error) = await ApiService.Instance.DeleteAndReadAsync($"{InventoryConstants.WAREHOUSES}/locations/{id}");
        if (isSuccess)
        {
            SetStatus("UI_MSG_DATA_LOADED_SUCCESS", Brushes.Green);
            await LoadDataAsync();
        }
        else
        {
            AlertWindow.ShowError(owner, error);
        }
    }

    #endregion

    private void ApplyFilters()
    {
        var searchText = SearchFilterTextBox?.Text?.Trim().ToLower() ?? string.Empty;
        var selectedStatusIndex = StatusFilterComboBox?.SelectedIndex ?? 0;

        var filtered = _allStorageLocations.Where(item =>
        {
            var matchesSearch = string.IsNullOrEmpty(searchText) ||
                                item.Name.ToLower().Contains(searchText) ||
                                item.LocationType.ToString().ToLower().Contains(searchText) ||
                                item.ParentName.ToLower().Contains(searchText);

            var matchesStatus = selectedStatusIndex switch
            {
                1 => item.SanitationStatusCode == "OK",
                2 => item.SanitationStatusCode == "WARNING",
                3 => item.SanitationStatusCode == "OVERDUE",
                _ => true
            };

            return matchesSearch && matchesStatus;
        }).ToList();

        StorageLocations.Clear();
        filtered.ForEach(StorageLocations.Add);
    }

    private void UpdateSanitationCounters()
    {
        TotalEquipmentCountTextBlock.Text = _allStorageLocations.Count.ToString();
        WarningCountTextBlock.Text = _allStorageLocations.Count(x => x.SanitationStatusCode == "WARNING").ToString();
        OverdueCountTextBlock.Text = _allStorageLocations.Count(x => x.SanitationStatusCode == "OVERDUE").ToString();
    }

    private void FilterChanged(object sender, RoutedEventArgs e)
    {
        ApplyFilters();
    }

    private async void CreateWarehouse_Click(object sender, RoutedEventArgs e)
    {
        var name = WarehouseNameTextBox.Text?.Trim();
        var address = WarehouseAddressTextBox.Text?.Trim();

        var selectedItem = WarehouseTypeComboBox.SelectedItem as ComboBoxItem;
        var rawTypeStr = selectedItem?.Tag?.ToString();

        var type = Enum.TryParse<WarehouseType>(rawTypeStr, out var parsedType)
            ? parsedType
            : WarehouseType.WAREHOUSE;

        if (string.IsNullOrWhiteSpace(name))
        {
            SetStatus("UI_ERR_INVALID_INPUT_FIELDS", Brushes.Red);
            return;
        }

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(InventoryConstants.WAREHOUSES, new
        {
            Name = name,
            Type = type,
            Address = address
        });

        if (isSuccess)
        {
            SetStatus("UI_STORAGE_MSG_WAREHOUSE_CREATED_SUCCESS", Brushes.Green);
            WarehouseNameTextBox.Clear();
            WarehouseAddressTextBox.Clear();
            WarehouseTypeComboBox.SelectedIndex = 0;
            await LoadDataAsync();
        }
        else
        {
            SetStatusRaw(contentOrError, Brushes.Red);
        }
    }

    private async void CreateChamber_Click(object sender, RoutedEventArgs e)
    {
        var name = ChamberNameTextBox.Text?.Trim();

        if (string.IsNullOrWhiteSpace(name) ||
            !TryParseDecimal(ChamberTempTextBox.Text, out var temp) ||
            !TryParseDecimal(ChamberHumidityTextBox.Text, out var humidity))
        {
            SetStatus("UI_ERR_INVALID_INPUT_FIELDS", Brushes.Red);
            return;
        }

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(AgingConstants.AGING_CHAMBERS, new
        {
            Name = name,
            TargetTemperature = temp,
            TargetHumidity = humidity
        });

        if (isSuccess)
        {
            SetStatus("UI_STORAGE_MSG_CHAMBER_CREATED_SUCCESS", Brushes.Green);
            ChamberNameTextBox.Clear();
            ChamberTempTextBox.Text = "12.0";
            ChamberHumidityTextBox.Text = "85.0";
            await LoadDataAsync();
        }
        else
        {
            SetStatusRaw(contentOrError, Brushes.Red);
        }
    }

    private async void OpenCreateStorageLocationModal_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new CreateStorageLocationWindow
        {
            Owner = Window.GetWindow(this)
        };

        if (dialog.ShowDialog() == true)
        {
            await LoadDataAsync();
            SetStatus("UI_MSG_DATA_LOADED_SUCCESS", Brushes.Green);
        }
    }

    private void SetStatus(string resourceKey, Brush color)
    {
        StatusUpdated?.Invoke(Get(resourceKey), color);
    }

    private void SetStatusFormatted(string resourceKey, Brush color, params object[] args)
    {
        var format = Get(resourceKey);
        StatusUpdated?.Invoke(string.Format(format, args), color);
    }

    private void SetStatusRaw(string rawText, Brush color)
    {
        StatusUpdated?.Invoke(rawText, color);
    }
}