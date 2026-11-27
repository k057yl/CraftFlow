using CraftFlow.SharedKernel.Constants.Endpoints;
using CraftFlow.SharedKernel.Dtos.Catalog.Nomenclature;
using CraftFlow.SharedKernel.Enums;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Services;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using static CraftFlow.Wpf.Services.LocalizationService;

namespace CraftFlow.Wpf.Windows;

public partial class CreateRecipeWindow : Window
{
    private readonly ObservableCollection<IngredientItemDto> _selectedIngredients = [];

    public CreateRecipeWindow(
        IEnumerable<LookupItem> unitsOfMeasure,
        IEnumerable<LookupItem> rawMaterials,
        IEnumerable<LookupItem> availableModules)
    {
        InitializeComponent();

        ProductUomComboBox.ItemsSource = unitsOfMeasure.ToList();
        RecipeRawMaterialComboBox.ItemsSource = rawMaterials.Where(r => !r.IsArchived).ToList();

        var modulesList = availableModules.ToList();
        RecipeModuleComboBox.ItemsSource = modulesList;
        if (modulesList.Count > 0)
        {
            RecipeModuleComboBox.SelectedIndex = 0;
        }

        AddedIngredientsListBox.ItemsSource = _selectedIngredients;
    }

    private void IsAgingRequired_Changed(object sender, RoutedEventArgs e)
    {
        if (DefaultMinAgingDaysTextBox != null)
        {
            DefaultMinAgingDaysTextBox.IsEnabled = IsAgingRequiredCheckBox.IsChecked ?? false;
        }
    }

    private void AddIngredient_Click(object sender, RoutedEventArgs e)
    {
        if (RecipeRawMaterialComboBox.SelectedItem is not LookupItem rawItem ||
            !TryParseDecimal(RecipeIngredientQuantityTextBox.Text, out var qty) ||
            qty <= 0)
        {
            AlertWindow.ShowError(this, Get("UI_ERR_INVALID_INPUT_FIELDS"));
            return;
        }

        var uomCode = rawItem.Code ?? string.Empty;
        var existing = _selectedIngredients.FirstOrDefault(i => i.RawMaterialId == rawItem.Id);

        if (existing != null)
        {
            var index = _selectedIngredients.IndexOf(existing);
            _selectedIngredients[index] = existing with { Quantity = existing.Quantity + qty };
        }
        else
        {
            _selectedIngredients.Add(new IngredientItemDto(rawItem.Id, rawItem.Name, uomCode, qty));
        }

        RecipeIngredientQuantityTextBox.Text = "1";
    }

    private void RemoveIngredient_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is IngredientItemDto ingredient)
        {
            _selectedIngredients.Remove(ingredient);
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        var productName = ProductNameTextBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(productName) ||
            ProductUomComboBox.SelectedValue is not Guid productUomId ||
            RecipeModuleComboBox.SelectedValue is not string moduleCode ||
            !Enum.TryParse<AppModule>(moduleCode, out var targetModule) ||
            !TryParseDecimal(RecipeOutputQuantityTextBox.Text, out var targetOutput) ||
            targetOutput <= 0 ||
            _selectedIngredients.Count == 0)
        {
            AlertWindow.ShowError(this, Get("UI_ERR_INVALID_INPUT_FIELDS"));
            return;
        }

        var (isProdSuccess, prodContentOrError) = await ApiService.Instance.PostAndReadAsync(CatalogConstants.PRODUCTS, new
        {
            Name = productName,
            UnitOfMeasureId = productUomId
        });

        if (!isProdSuccess)
        {
            AlertWindow.ShowError(this, prodContentOrError);
            return;
        }

        if (!Guid.TryParse(prodContentOrError.Replace("\"", "").Trim(), out var productId))
        {
            DialogResult = true;
            return;
        }

        var isAgingRequired = IsAgingRequiredCheckBox.IsChecked ?? false;
        int? defaultMinAgingDays = isAgingRequired && int.TryParse(DefaultMinAgingDaysTextBox.Text.Trim(), out var days)
            ? days
            : null;

        var ingredientsPayload = _selectedIngredients
            .Select(i => new { RawMaterialId = i.RawMaterialId, Quantity = i.Quantity })
            .ToArray();

        var (isRecipeSuccess, recipeContentOrError) = await ApiService.Instance.PostAndReadAsync(CatalogConstants.RECIPES, new
        {
            ProductId = productId,
            Name = productName,
            TargetModule = targetModule,
            TargetOutputQuantity = targetOutput,
            IsAgingRequired = isAgingRequired,
            DefaultMinAgingDays = defaultMinAgingDays,
            Ingredients = ingredientsPayload
        });

        if (isRecipeSuccess)
        {
            DialogResult = true;
        }
        else
        {
            AlertWindow.ShowError(this, recipeContentOrError);
        }
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
}