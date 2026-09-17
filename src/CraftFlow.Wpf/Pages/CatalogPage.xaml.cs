using CraftFlow.Api.Modules.Catalog;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Services;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static CraftFlow.Wpf.Models.Catalogs.CatalogDtos;

namespace CraftFlow.Wpf.Pages;

public partial class CatalogPage : Page
{
    public ObservableCollection<LookupItem> UnitsOfMeasure { get; } = [];
    public ObservableCollection<LookupItem> RawMaterials { get; } = [];
    public ObservableCollection<LookupItem> Products { get; } = [];
    public ObservableCollection<LookupItem> Recipes { get; } = [];

    private readonly ObservableCollection<IngredientItemDto> _selectedIngredients = [];

    public CatalogPage()
    {
        InitializeComponent();

        RawUomComboBox.ItemsSource = UnitsOfMeasure;
        ProductUomComboBox.ItemsSource = UnitsOfMeasure;
        RecipeProductComboBox.ItemsSource = Products;
        RecipeRawMaterialComboBox.ItemsSource = RawMaterials;
        AddedIngredientsListBox.ItemsSource = _selectedIngredients;

        UomDataGrid.ItemsSource = UnitsOfMeasure;
        RawMaterialsDataGrid.ItemsSource = RawMaterials;
        ProductsDataGrid.ItemsSource = Products;
        RecipesDataGrid.ItemsSource = Recipes;

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
            UnitsOfMeasure.Clear();
            uoms?.ForEach(u => UnitsOfMeasure.Add(new LookupItem(u.Id, u.Name, u.Code)));

            var raw = await ApiService.Instance.GetAsync<List<LookupDto>>(CatalogConstants.RAW_MATERIALS);
            RawMaterials.Clear();
            raw?.ForEach(r => RawMaterials.Add(new LookupItem(r.Id, r.Name)));

            var prods = await ApiService.Instance.GetAsync<List<LookupDto>>(CatalogConstants.PRODUCTS);
            Products.Clear();
            prods?.ForEach(p => Products.Add(new LookupItem(p.Id, p.Name)));

            var recs = await ApiService.Instance.GetAsync<List<LookupDto>>(CatalogConstants.RECIPES);
            Recipes.Clear();
            recs?.ForEach(r => Recipes.Add(new LookupItem(r.Id, r.Name)));

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
            _selectedIngredients.Add(new IngredientItemDto(rawItem.Id, rawItem.Name, string.Empty, qty));
            RecipeIngredientQuantityTextBox.Text = "1";
        }
        else
        {
            SetStatus("UI_INVALID_INPUT_FIELDS", Brushes.Red);
        }
    }

    // --- СОЗДАНИЕ ---

    private async void CreateUom_Click(object sender, RoutedEventArgs e)
    {
        var name = UomNameTextBox.Text?.Trim();
        var code = UomCodeTextBox.Text?.Trim();

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(code))
        {
            SetStatus("UI_INVALID_INPUT_FIELDS", Brushes.Red);
            return;
        }

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(CatalogConstants.UOM, new
        {
            Name = name,
            Code = code
        });

        if (isSuccess)
        {
            UomNameTextBox.Clear();
            UomCodeTextBox.Clear();
            await LoadDataAsync();
        }
        else
        {
            SetStatusRaw(contentOrError, Brushes.Red);
        }
    }

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

    private async void CreateProduct_Click(object sender, RoutedEventArgs e)
    {
        if (ProductUomComboBox.SelectedValue is not Guid uomId || string.IsNullOrWhiteSpace(ProductNameTextBox.Text))
        {
            SetStatus("UI_INVALID_INPUT_FIELDS", Brushes.Red);
            return;
        }

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(CatalogConstants.PRODUCTS, new
        {
            Name = ProductNameTextBox.Text.Trim(),
            UnitOfMeasureId = uomId
        });

        if (isSuccess)
        {
            ProductNameTextBox.Clear();
            await LoadDataAsync();
        }
        else
        {
            SetStatusRaw(contentOrError, Brushes.Red);
        }
    }

    private async void CreateRecipe_Click(object sender, RoutedEventArgs e)
    {
        if (RecipeProductComboBox.SelectedValue is not Guid productId ||
            !TryParseDecimal(RecipeOutputQuantityTextBox.Text, out var targetOutput) ||
            _selectedIngredients.Count == 0 ||
            string.IsNullOrWhiteSpace(RecipeNameTextBox.Text))
        {
            SetStatus("UI_INVALID_INPUT_FIELDS", Brushes.Red);
            return;
        }

        var isAgingRequired = IsAgingRequiredCheckBox.IsChecked ?? false;
        int? defaultMinAgingDays = isAgingRequired && int.TryParse(DefaultMinAgingDaysTextBox.Text.Trim(), out var days) ? days : null;

        var ingredientsPayload = _selectedIngredients
            .Select(i => new { RawMaterialId = i.RawMaterialId, Quantity = i.Quantity })
            .ToArray();

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(CatalogConstants.RECIPES, new
        {
            ProductId = productId,
            Name = RecipeNameTextBox.Text.Trim(),
            TargetOutputQuantity = targetOutput,
            IsAgingRequired = isAgingRequired,
            DefaultMinAgingDays = defaultMinAgingDays,
            Ingredients = ingredientsPayload
        });

        if (isSuccess)
        {
            RecipeNameTextBox.Clear();
            _selectedIngredients.Clear();
            await LoadDataAsync();
        }
        else
        {
            SetStatusRaw(contentOrError, Brushes.Red);
        }
    }

    // --- УДАЛЕНИЕ ---

    private async void DeleteUom_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Guid id)
        {
            await ApiService.Instance.DeleteAsync($"{CatalogConstants.UOM}/{id}");
            await LoadDataAsync();
        }
    }

    private async void DeleteRawMaterial_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Guid id)
        {
            await ApiService.Instance.DeleteAsync($"{CatalogConstants.RAW_MATERIALS}/{id}");
            await LoadDataAsync();
        }
    }

    private async void DeleteProduct_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Guid id)
        {
            await ApiService.Instance.DeleteAsync($"{CatalogConstants.PRODUCTS}/{id}");
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

    // --- АВТОСИДИНГ ПРЕСЕТОВ ЕД. ИЗМЕРЕНИЯ ---

    private void SeedUomPreset_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.ContextMenu != null)
        {
            btn.ContextMenu.IsOpen = true;
        }
    }

    private async Task ExecuteUomSeedAsync(int presetType)
    {
        var (isSuccess, response) = await ApiService.Instance.PostAndReadAsync($"{CatalogConstants.UOM}/seed", new
        {
            Preset = presetType
        });

        if (isSuccess)
        {
            await LoadDataAsync();
            SetStatus("UI_DATA_LOADED_SUCCESS", Brushes.Green);
        }
        else
        {
            SetStatusRaw(response, Brushes.Red);
        }
    }

    private async void SeedMetric_Click(object sender, RoutedEventArgs e) => await ExecuteUomSeedAsync(0);   // Metric
    private async void SeedImperial_Click(object sender, RoutedEventArgs e) => await ExecuteUomSeedAsync(1); // Imperial
    private async void SeedFull_Click(object sender, RoutedEventArgs e) => await ExecuteUomSeedAsync(2);     // Full

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