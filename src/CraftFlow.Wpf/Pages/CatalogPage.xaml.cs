using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Services;

namespace CraftFlow.Wpf.Pages;

public record IngredientItemDto(Guid RawMaterialId, string Name, decimal Quantity)
{
    public string DisplayInfo => $"{Name} — {Quantity}";
}

public partial class CatalogPage : Page
{
    public ObservableCollection<LookupItem> UnitsOfMeasure { get; } = [];
    public ObservableCollection<LookupItem> RawMaterials { get; } = [];
    public ObservableCollection<LookupItem> Products { get; } = [];

    // Временный список ингредиентов для рецепта
    private readonly ObservableCollection<IngredientItemDto> _selectedIngredients = [];

    public CatalogPage()
    {
        InitializeComponent();

        RawUomComboBox.ItemsSource = UnitsOfMeasure;
        ProductUomComboBox.ItemsSource = UnitsOfMeasure;
        RecipeProductComboBox.ItemsSource = Products;
        RecipeRawMaterialComboBox.ItemsSource = RawMaterials;
        AddedIngredientsListBox.ItemsSource = _selectedIngredients;

        Loaded += async (s, e) => await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var uoms = await ApiService.Instance.GetAsync<List<LookupDto>>(Endpoints.UOM);
            UnitsOfMeasure.Clear();
            uoms?.ForEach(u => UnitsOfMeasure.Add(new LookupItem(u.Id, $"{u.Name} ({u.Code})")));

            var raw = await ApiService.Instance.GetAsync<List<LookupDto>>(Endpoints.RAW_MATERIALS);
            RawMaterials.Clear();
            raw?.ForEach(r => RawMaterials.Add(new LookupItem(r.Id, r.Name)));

            var prods = await ApiService.Instance.GetAsync<List<LookupDto>>(Endpoints.PRODUCTS);
            Products.Clear();
            prods?.ForEach(p => Products.Add(new LookupItem(p.Id, p.Name)));

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
            _selectedIngredients.Add(new IngredientItemDto(rawItem.Id, rawItem.Name, qty));
        }
    }

    private async void CreateUom_Click(object sender, RoutedEventArgs e)
    {
        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(Endpoints.UOM, new
        {
            Name = UomNameTextBox.Text,
            Code = UomCodeTextBox.Text
        });

        if (isSuccess)
        {
            SetStatus(UiConstants.Messages.UOM_CREATED_SUCCESS, Brushes.Green);
            await LoadDataAsync();
        }
        else
        {
            SetStatus(contentOrError, Brushes.Red);
        }
    }

    private async void CreateRawMaterial_Click(object sender, RoutedEventArgs e)
    {
        if (RawUomComboBox.SelectedValue is not Guid uomId)
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
            return;
        }

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(Endpoints.RAW_MATERIALS, new
        {
            Name = RawMaterialNameTextBox.Text,
            UnitOfMeasureId = uomId
        });

        if (isSuccess)
        {
            SetStatus(UiConstants.Messages.RAW_MATERIAL_CREATED_SUCCESS, Brushes.Green);
            await LoadDataAsync();
        }
        else
        {
            SetStatus(contentOrError, Brushes.Red);
        }
    }

    private async void CreateProduct_Click(object sender, RoutedEventArgs e)
    {
        if (ProductUomComboBox.SelectedValue is not Guid uomId)
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
            return;
        }

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(Endpoints.PRODUCTS, new
        {
            Name = ProductNameTextBox.Text,
            UnitOfMeasureId = uomId
        });

        if (isSuccess)
        {
            SetStatus(UiConstants.Messages.PRODUCT_CREATED_SUCCESS, Brushes.Green);
            await LoadDataAsync();
        }
        else
        {
            SetStatus(contentOrError, Brushes.Red);
        }
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

        var ingredientsPayload = _selectedIngredients
            .Select(i => new { RawMaterialId = i.RawMaterialId, Quantity = i.Quantity })
            .ToArray();

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(Endpoints.RECIPES, new
        {
            ProductId = productId,
            Name = RecipeNameTextBox.Text,
            TargetOutputQuantity = targetOutput,
            Ingredients = ingredientsPayload
        });

        if (isSuccess)
        {
            SetStatus($"{UiConstants.Messages.RECIPE_CREATED_SUCCESS} ID: {contentOrError}", Brushes.Green);
            _selectedIngredients.Clear();
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