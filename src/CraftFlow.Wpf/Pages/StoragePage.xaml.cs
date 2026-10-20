using CraftFlow.Api.Modules.Aging;
using CraftFlow.Api.Modules.Inventory;
using CraftFlow.SharedKernel.Constants;
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
using System.Windows.Input;
using System.Windows.Media;

namespace CraftFlow.Wpf.Pages;

public partial class StoragePage : Page
{
    private List<StorageLocationViewItem> _allStorageLocations = [];

    public ObservableCollection<LookupItem> Warehouses { get; } = [];
    public ObservableCollection<LookupItem> Chambers { get; } = [];
    public ObservableCollection<StorageLocationViewItem> StorageLocations { get; } = [];

    public StoragePage()
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

                string statusText = "ОК";
                Brush statusBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E7D32"));
                Brush backgroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8F5E9"));
                string code = "OK";

                if (l.NextWashDueDate.HasValue)
                {
                    if (l.NextWashDueDate.Value < now)
                    {
                        statusText = "🚨 ПРОСРОЧЕНО";
                        statusBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C62828"));
                        backgroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEBEE"));
                        code = "OVERDUE";
                    }
                    else if ((l.NextWashDueDate.Value - now).TotalDays <= 2)
                    {
                        statusText = "⚠️ СКОРО МОЙКА";
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

            SetStatus(UiConstants.Messages.DATA_LOADED_SUCCESS, Brushes.Green);
        }
        catch (Exception ex)
        {
            SetStatusFormatted(UiConstants.Messages.DATA_LOAD_ERROR, Brushes.Red, ex.Message);
        }
    }

    #region Регламенты моек (Шестеренки)

    private async void OpenSanitationWindow_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is StorageLocationViewItem item)
        {
            var dialog = new SanitationRuleWindow(item.Id, item.Name, SanitationTargetType.STORAGE_LOCATION)
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true)
            {
                await LoadDataAsync();
            }
        }
    }

    private async void OpenWarehouseSanitation_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is LookupItem item)
        {
            var dialog = new SanitationRuleWindow(item.Id, item.Name, SanitationTargetType.WAREHOUSE)
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true)
            {
                await LoadDataAsync();
            }
        }
    }

    private async void OpenChamberSanitation_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is LookupItem item)
        {
            var dialog = new SanitationRuleWindow(item.Id, item.Name, SanitationTargetType.AGING_CHAMBER)
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true)
            {
                await LoadDataAsync();
            }
        }
    }

    #endregion

    #region Просмотр истории и состояния (Дабл-клик по строке)

    private async void WarehousesDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (WarehousesDataGrid.SelectedItem is LookupItem item)
        {
            var detailsWindow = new SanitationDetailsWindow(item.Id, item.Name, SanitationTargetType.WAREHOUSE)
            {
                Owner = Window.GetWindow(this)
            };
            detailsWindow.ShowDialog();
            await LoadDataAsync();
        }
    }

    private async void ChambersDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (ChambersDataGrid.SelectedItem is LookupItem item)
        {
            var detailsWindow = new SanitationDetailsWindow(item.Id, item.Name, SanitationTargetType.AGING_CHAMBER)
            {
                Owner = Window.GetWindow(this)
            };
            detailsWindow.ShowDialog();
            await LoadDataAsync();
        }
    }

    private async void StorageLocationsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (StorageLocationsDataGrid.SelectedItem is StorageLocationViewItem item)
        {
            var detailsWindow = new SanitationDetailsWindow(item.Id, item.Name, SanitationTargetType.STORAGE_LOCATION)
            {
                Owner = Window.GetWindow(this)
            };
            detailsWindow.ShowDialog();
            await LoadDataAsync();
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
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
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
            SetStatus(UiConstants.Messages.WAREHOUSE_CREATED_SUCCESS, Brushes.Green);
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
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
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
            SetStatus(UiConstants.Messages.AGING_CHAMBER_CREATED_SUCCESS, Brushes.Green);
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
            SetStatus(UiConstants.Messages.DATA_LOADED_SUCCESS, Brushes.Green);
        }
    }

    private async void DeleteWarehouse_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Guid id)
        {
            var (isSuccess, error) = await ApiService.Instance.DeleteAndReadAsync($"{InventoryConstants.WAREHOUSES}/{id}");
            if (isSuccess)
            {
                SetStatus(UiConstants.Messages.DATA_LOADED_SUCCESS, Brushes.Green);
                await LoadDataAsync();
            }
            else
            {
                SetStatusRaw(error, Brushes.Red);
            }
        }
    }

    private async void DeleteChamber_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Guid id)
        {
            var (isSuccess, error) = await ApiService.Instance.DeleteAndReadAsync($"{AgingConstants.AGING_CHAMBERS}/{id}");
            if (isSuccess)
            {
                SetStatus(UiConstants.Messages.DATA_LOADED_SUCCESS, Brushes.Green);
                await LoadDataAsync();
            }
            else
            {
                SetStatusRaw(error, Brushes.Red);
            }
        }
    }

    private async void DeleteStorageLocation_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Guid id)
        {
            var (isSuccess, error) = await ApiService.Instance.DeleteAndReadAsync($"{InventoryConstants.WAREHOUSES}/locations/{id}");
            if (isSuccess)
            {
                SetStatus(UiConstants.Messages.DATA_LOADED_SUCCESS, Brushes.Green);
                await LoadDataAsync();
            }
            else
            {
                SetStatusRaw(error, Brushes.Red);
            }
        }
    }

    private async void OpenExecutionSanitation_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is StorageLocationViewItem item)
        {
            var window = new SanitationExecutionWindow(item.Id, item.Name, SanitationTargetType.STORAGE_LOCATION)
            {
                Owner = Window.GetWindow(this)
            };

            if (window.ShowDialog() == true)
            {
                await LoadDataAsync();
            }
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

public class StorageLocationViewItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public StorageLocationType LocationType { get; set; }
    public string ParentName { get; set; } = string.Empty;
    public string VolumeInfo { get; set; } = string.Empty;
    public string LastWashedText { get; set; } = string.Empty;
    public string NextWashText { get; set; } = string.Empty;
    public string SanitationStatusCode { get; set; } = "OK";
    public string SanitationStatusText { get; set; } = "ОК";
    public Brush StatusForeground { get; set; } = Brushes.Green;
    public Brush StatusBackground { get; set; } = Brushes.LightGreen;
}