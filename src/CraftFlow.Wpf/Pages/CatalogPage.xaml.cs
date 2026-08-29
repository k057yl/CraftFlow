using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Services;

namespace CraftFlow.Wpf.Pages;

public record IngredientItemDto(Guid RawMaterialId, string Name, string Code, decimal Quantity)
{
    public string DisplayInfo => $"{Name} — {Quantity}";
}

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

    private async Task LoadDataAsync()
    {
        try
        {
            var uoms = await ApiService.Instance.GetAsync<List<LookupDto>>(Endpoints.UOM);
            UnitsOfMeasure.Clear();
            uoms?.ForEach(u => UnitsOfMeasure.Add(new LookupItem(u.Id, u.Name, u.Code)));

            var raw = await ApiService.Instance.GetAsync<List<LookupDto>>(Endpoints.RAW_MATERIALS);
            RawMaterials.Clear();
            raw?.ForEach(r => RawMaterials.Add(new LookupItem(r.Id, r.Name)));

            var prods = await ApiService.Instance.GetAsync<List<LookupDto>>(Endpoints.PRODUCTS);
            Products.Clear();
            prods?.ForEach(p => Products.Add(new LookupItem(p.Id, p.Name)));

            var recs = await ApiService.Instance.GetAsync<List<LookupDto>>(Endpoints.RECIPES);
            Recipes.Clear();
            recs?.ForEach(r => Recipes.Add(new LookupItem(r.Id, r.Name)));

            SetStatus(UiConstants.Messages.DATA_LOADED_SUCCESS, Brushes.Green);
        }
        catch (Exception ex)
        {
            SetStatus($"{UiConstants.Messages.DATA_LOAD_ERROR}: {ex.Message}", Brushes.Red);
        }
    }

    private void AddIngredient_Click(object sender, RoutedEventArgs e)
    {
        if (RecipeRawMaterialComboBox.SelectedItem is LookupItem rawItem &&
            decimal.TryParse(RecipeIngredientQuantityTextBox.Text, out var qty) && qty > 0)
        {
            _selectedIngredients.Add(new IngredientItemDto(rawItem.Id, rawItem.Name, string.Empty, qty));
        }
    }

    // --- СОЗДАНИЕ ---

    private async void CreateUom_Click(object sender, RoutedEventArgs e)
    {
        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(Endpoints.UOM, new
        {
            Name = UomNameTextBox.Text,
            Code = UomCodeTextBox.Text
        });

        if (isSuccess) { await LoadDataAsync(); } else { SetStatus(contentOrError, Brushes.Red); }
    }

    private async void CreateRawMaterial_Click(object sender, RoutedEventArgs e)
    {
        if (RawUomComboBox.SelectedValue is not Guid uomId) return;

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(Endpoints.RAW_MATERIALS, new
        {
            Name = RawMaterialNameTextBox.Text,
            UnitOfMeasureId = uomId
        });

        if (isSuccess) { await LoadDataAsync(); } else { SetStatus(contentOrError, Brushes.Red); }
    }

    private async void CreateProduct_Click(object sender, RoutedEventArgs e)
    {
        if (ProductUomComboBox.SelectedValue is not Guid uomId) return;

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(Endpoints.PRODUCTS, new
        {
            Name = ProductNameTextBox.Text,
            UnitOfMeasureId = uomId
        });

        if (isSuccess) { await LoadDataAsync(); } else { SetStatus(contentOrError, Brushes.Red); }
    }

    private async void CreateRecipe_Click(object sender, RoutedEventArgs e)
    {
        if (RecipeProductComboBox.SelectedValue is not Guid productId ||
            !decimal.TryParse(RecipeOutputQuantityTextBox.Text, out var targetOutput) ||
            _selectedIngredients.Count == 0)
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
            return;
        }

        var isAgingRequired = IsAgingRequiredCheckBox.IsChecked ?? false;
        int? defaultMinAgingDays = isAgingRequired && int.TryParse(DefaultMinAgingDaysTextBox.Text, out var days) ? days : null;

        var ingredientsPayload = _selectedIngredients
            .Select(i => new { RawMaterialId = i.RawMaterialId, Quantity = i.Quantity })
            .ToArray();

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(Endpoints.RECIPES, new
        {
            ProductId = productId,
            Name = RecipeNameTextBox.Text,
            TargetOutputQuantity = targetOutput,
            IsAgingRequired = isAgingRequired,
            DefaultMinAgingDays = defaultMinAgingDays,
            Ingredients = ingredientsPayload
        });

        if (isSuccess)
        {
            _selectedIngredients.Clear();
            await LoadDataAsync();
        }
        else
        {
            SetStatus(contentOrError, Brushes.Red);
        }
    }

    // --- УДАЛЕНИЕ ---

    private async void DeleteUom_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Guid id)
        {
            await ApiService.Instance.DeleteAsync($"{Endpoints.UOM}/{id}");
            await LoadDataAsync();
        }
    }

    private async void DeleteRawMaterial_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Guid id)
        {
            await ApiService.Instance.DeleteAsync($"{Endpoints.RAW_MATERIALS}/{id}");
            await LoadDataAsync();
        }
    }

    private async void DeleteProduct_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Guid id)
        {
            await ApiService.Instance.DeleteAsync($"{Endpoints.PRODUCTS}/{id}");
            await LoadDataAsync();
        }
    }

    private async void DeleteRecipe_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Guid id)
        {
            await ApiService.Instance.DeleteAsync($"{Endpoints.RECIPES}/{id}");
            await LoadDataAsync();
        }
    }

    private void SetStatus(string msg, Brush color)
    {
        StatusTextBlock.Foreground = color;
        StatusTextBlock.Text = LocalizationService.Get(msg);
    }
}