using CraftFlow.Api.Modules.Aging;
using CraftFlow.Api.Modules.Catalog;
using CraftFlow.Api.Modules.Inventory;
using CraftFlow.SharedKernel.Dtos.Catalog.Nomenclature;
using CraftFlow.SharedKernel.Dtos.Common;
using CraftFlow.SharedKernel.Dtos.Inventory;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Services;
using CraftFlow.Wpf.Windows;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace CraftFlow.Wpf.Pages;

public partial class CatalogPage : Page
{
    public ObservableCollection<LookupItem> UnitsOfMeasure { get; } = [];
    public ObservableCollection<LookupItem> RawMaterials { get; } = [];
    public ObservableCollection<RecipeDto> Recipes { get; } = [];
    private readonly ObservableCollection<IngredientItemDto> _selectedIngredients = [];

    public ObservableCollection<LookupItem> Warehouses { get; } = [];
    public ObservableCollection<LookupItem> Chambers { get; } = [];
    public ObservableCollection<StorageLocationViewItem> StorageLocations { get; } = [];

    public CatalogPage()
    {
        InitializeComponent();

        RawUomComboBox.ItemsSource = UnitsOfMeasure;
        ProductUomComboBox.ItemsSource = UnitsOfMeasure;
        RecipeRawMaterialComboBox.ItemsSource = RawMaterials;
        AddedIngredientsListBox.ItemsSource = _selectedIngredients;

        RawMaterialsDataGrid.ItemsSource = RawMaterials;
        RecipesDataGrid.ItemsSource = Recipes;

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
            var uoms = await ApiService.Instance.GetAsync<List<LookupDto>>(CatalogConstants.UOM);

            if (uoms == null || uoms.Count == 0)
            {
                var (isSuccess, _) = await ApiService.Instance.PostAndReadAsync($"{CatalogConstants.UOM}/seed", new { });
                if (isSuccess)
                {
                    uoms = await ApiService.Instance.GetAsync<List<LookupDto>>(CatalogConstants.UOM);
                }
            }

            UnitsOfMeasure.Clear();
            uoms?.ForEach(u => UnitsOfMeasure.Add(new LookupItem(u.Id, u.Name, u.Code)));

            var raw = await ApiService.Instance.GetAsync<List<RawMaterialDto>>(CatalogConstants.RAW_MATERIALS);
            RawMaterials.Clear();
            raw?.ForEach(r =>
            {
                var displayName = !string.IsNullOrWhiteSpace(r.UnitOfMeasureCode) ? $"{r.Name} ({r.UnitOfMeasureCode})" : r.Name;
                RawMaterials.Add(new LookupItem(r.Id, displayName, r.UnitOfMeasureCode));
            });

            var recs = await ApiService.Instance.GetAsync<List<RecipeDto>>(CatalogConstants.RECIPES);
            Recipes.Clear();
            recs?.ForEach(r => Recipes.Add(r));

            var warehouses = await ApiService.Instance.GetAsync<List<LookupDto>>(InventoryConstants.WAREHOUSES);
            Warehouses.Clear();
            warehouses?.ForEach(w => Warehouses.Add(new LookupItem(w.Id, w.Name)));

            var chambers = await ApiService.Instance.GetAsync<List<LookupDto>>(AgingConstants.AGING_CHAMBERS);
            Chambers.Clear();
            chambers?.ForEach(c => Chambers.Add(new LookupItem(c.Id, c.Name)));

            var locations = await ApiService.Instance.GetAsync<List<StorageLocationDto>>($"{InventoryConstants.WAREHOUSES}/locations");
            StorageLocations.Clear();
            locations?.ForEach(l =>
            {
                string parentName = !string.IsNullOrWhiteSpace(l.WarehouseName)
                    ? $"🏠 {l.WarehouseName}"
                    : (!string.IsNullOrWhiteSpace(l.ChamberName) ? $"❄ {l.ChamberName}" : "—");

                StorageLocations.Add(new StorageLocationViewItem
                {
                    Id = l.Id,
                    Name = l.Name,
                    LocationType = l.LocationType,
                    VolumeInfo = l.VolumeInfo,
                    ParentName = parentName
                });
            });

            SetStatus("UI_DATA_LOADED_SUCCESS", Brushes.Green);
        }
        catch (Exception ex)
        {
            SetStatusFormatted("UI_DATA_LOAD_ERROR", Brushes.Red, ex.Message);
        }
    }

    private void AddIngredient_Click(object sender, RoutedEventArgs e)
    {
        if (RecipeRawMaterialComboBox.SelectedItem is LookupItem rawItem &&
            TryParseDecimal(RecipeIngredientQuantityTextBox.Text, out var qty) && qty > 0)
        {
            var uomCode = rawItem.Code ?? string.Empty;
            _selectedIngredients.Add(new IngredientItemDto(rawItem.Id, rawItem.Name, uomCode, qty));
            RecipeIngredientQuantityTextBox.Text = "1";
        }
        else
        {
            SetStatus("UI_INVALID_INPUT_FIELDS", Brushes.Red);
        }
    }

    private void OpenRecipeDetails_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Hyperlink link && link.DataContext is RecipeDto recipe)
        {
            var dialog = new RecipeDetailsWindow(recipe)
            {
                Owner = Window.GetWindow(this)
            };
            dialog.ShowDialog();
        }
    }

    // --- СОЗДАНИЕ ---

    private async void CreateRawMaterial_Click(object sender, RoutedEventArgs e)
    {
        if (RawUomComboBox.SelectedValue is not Guid uomId || string.IsNullOrWhiteSpace(RawMaterialNameTextBox.Text))
        {
            SetStatus("UI_INVALID_INPUT_FIELDS", Brushes.Red);
            return;
        }

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(CatalogConstants.RAW_MATERIALS, new
        {
            Name = RawMaterialNameTextBox.Text.Trim(),
            UnitOfMeasureId = uomId
        });

        if (isSuccess)
        {
            RawMaterialNameTextBox.Clear();
            await LoadDataAsync();
        }
        else
        {
            SetStatusRaw(contentOrError, Brushes.Red);
        }
    }

    private async void CreateRecipe_Click(object sender, RoutedEventArgs e)
    {
        var productName = ProductNameTextBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(productName) ||
            ProductUomComboBox.SelectedValue is not Guid productUomId ||
            !TryParseDecimal(RecipeOutputQuantityTextBox.Text, out var targetOutput) ||
            _selectedIngredients.Count == 0)
        {
            SetStatus("UI_INVALID_INPUT_FIELDS", Brushes.Red);
            return;
        }

        var (isProdSuccess, prodContentOrError) = await ApiService.Instance.PostAndReadAsync(CatalogConstants.PRODUCTS, new
        {
            Name = productName,
            UnitOfMeasureId = productUomId
        });

        if (!isProdSuccess)
        {
            SetStatusRaw(prodContentOrError, Brushes.Red);
            return;
        }

        Guid productId;
        if (!Guid.TryParse(prodContentOrError.Replace("\"", "").Trim(), out productId))
        {
            await LoadDataAsync();
            return;
        }

        var isAgingRequired = IsAgingRequiredCheckBox.IsChecked ?? false;
        int? defaultMinAgingDays = isAgingRequired && int.TryParse(DefaultMinAgingDaysTextBox.Text.Trim(), out var days) ? days : null;

        var ingredientsPayload = _selectedIngredients
            .Select(i => new { RawMaterialId = i.RawMaterialId, Quantity = i.Quantity })
            .ToArray();

        var (isRecipeSuccess, recipeContentOrError) = await ApiService.Instance.PostAndReadAsync(CatalogConstants.RECIPES, new
        {
            ProductId = productId,
            Name = productName,
            TargetOutputQuantity = targetOutput,
            IsAgingRequired = isAgingRequired,
            DefaultMinAgingDays = defaultMinAgingDays,
            Ingredients = ingredientsPayload
        });

        if (isRecipeSuccess)
        {
            ProductNameTextBox.Clear();
            _selectedIngredients.Clear();
            await LoadDataAsync();
        }
        else
        {
            SetStatusRaw(recipeContentOrError, Brushes.Red);
        }
    }

    private async void CreateWarehouse_Click(object sender, RoutedEventArgs e)
    {
        var name = WarehouseNameTextBox.Text?.Trim();
        var address = WarehouseAddressTextBox.Text?.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            SetStatus("UI_INVALID_INPUT_FIELDS", Brushes.Red);
            return;
        }

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(InventoryConstants.WAREHOUSES, new
        {
            Name = name,
            Address = address
        });

        if (isSuccess)
        {
            SetStatus("UI_WAREHOUSE_CREATED_SUCCESS", Brushes.Green);
            WarehouseNameTextBox.Clear();
            WarehouseAddressTextBox.Clear();
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
            SetStatus("UI_INVALID_INPUT_FIELDS", Brushes.Red);
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
            SetStatus("UI_AGING_CHAMBER_CREATED_SUCCESS", Brushes.Green);
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
            SetStatus("UI_DATA_LOADED_SUCCESS", Brushes.Green);
        }
    }

    // --- УДАЛЕНИЕ ---

    private async void DeleteRawMaterial_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Guid id)
        {
            await ApiService.Instance.DeleteAsync($"{CatalogConstants.RAW_MATERIALS}/{id}");
            await LoadDataAsync();
        }
    }

    private async void DeleteRecipe_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Guid id)
        {
            await ApiService.Instance.DeleteAsync($"{CatalogConstants.RECIPES}/{id}");
            await LoadDataAsync();
        }
    }

    private async void DeleteWarehouse_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Guid id)
        {
            var (isSuccess, error) = await ApiService.Instance.DeleteAndReadAsync($"{InventoryConstants.WAREHOUSES}/{id}");
            if (isSuccess)
            {
                SetStatus("UI_DATA_LOADED_SUCCESS", Brushes.Green);
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
                SetStatus("UI_DATA_LOADED_SUCCESS", Brushes.Green);
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
                SetStatus("UI_DATA_LOADED_SUCCESS", Brushes.Green);
                await LoadDataAsync();
            }
            else
            {
                SetStatusRaw(error, Brushes.Red);
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
    public string LocationType { get; set; } = string.Empty;
    public string ParentName { get; set; } = string.Empty;
    public string VolumeInfo { get; set; } = string.Empty;
}