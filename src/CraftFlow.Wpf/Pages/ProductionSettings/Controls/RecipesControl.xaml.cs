using CraftFlow.Api.Modules.Catalog;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Dtos.Catalog.Nomenclature;
using CraftFlow.SharedKernel.Dtos.Common;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Services;
using CraftFlow.Wpf.Services.Converters;
using CraftFlow.Wpf.Windows;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace CraftFlow.Wpf.Pages.ProductionSettings.Controls;

public partial class RecipesControl : UserControl
{
    public event Action<string, Brush>? StatusUpdated;

    public ObservableCollection<LookupItem> UnitsOfMeasure { get; } = [];
    public ObservableCollection<LookupItem> RawMaterials { get; } = [];
    public ObservableCollection<RecipeDto> Recipes { get; } = [];
    private readonly ObservableCollection<IngredientItemDto> _selectedIngredients = [];

    public RecipesControl()
    {
        InitializeComponent();

        RawUomComboBox.ItemsSource = UnitsOfMeasure;
        ProductUomComboBox.ItemsSource = UnitsOfMeasure;
        RecipeRawMaterialComboBox.ItemsSource = RawMaterials;
        AddedIngredientsListBox.ItemsSource = _selectedIngredients;

        RawMaterialsDataGrid.ItemsSource = RawMaterials;
        RecipesDataGrid.ItemsSource = Recipes;

        Loaded += async (s, e) =>
        {
            ApplyRolePermissions();
            await LoadDataAsync();
        };
    }

    private void ApplyRolePermissions()
    {
        var userRole = ApiService.Instance.CurrentUser?.Role;
        bool canManageArchived = userRole == TenantRole.OWNER || userRole == TenantRole.ACCOUNTANT;
        var targetVisibility = canManageArchived ? Visibility.Visible : Visibility.Collapsed;

        RawMaterialDeleteColumn.Visibility = targetVisibility;
        RecipeDeleteColumn.Visibility = targetVisibility;

        ShowArchivedRawCheckBox.Visibility = targetVisibility;
        ShowArchivedRecipesCheckBox.Visibility = targetVisibility;
    }

    private async void ShowArchived_Changed(object sender, RoutedEventArgs e)
    {
        if (IsLoaded)
        {
            await LoadDataAsync();
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

            bool showRawArchived = ShowArchivedRawCheckBox.IsChecked ?? false;
            var rawUrl = $"{CatalogConstants.RAW_MATERIALS}?includeArchived={showRawArchived}";
            var raw = await ApiService.Instance.GetAsync<List<RawMaterialDto>>(rawUrl);

            RawMaterials.Clear();
            raw?.ForEach(r =>
            {
                var displayName = !string.IsNullOrWhiteSpace(r.UnitOfMeasureCode) ? $"{r.Name} ({r.UnitOfMeasureCode})" : r.Name;
                RawMaterials.Add(new LookupItem(r.Id, displayName, r.UnitOfMeasureCode));
            });

            bool showRecipesArchived = ShowArchivedRecipesCheckBox.IsChecked ?? false;
            var recipesUrl = $"{CatalogConstants.RECIPES}?includeArchived={showRecipesArchived}";
            var recs = await ApiService.Instance.GetAsync<List<RecipeDto>>(recipesUrl);

            Recipes.Clear();
            recs?.ForEach(r => Recipes.Add(r));

            SetStatus(UiConstants.Messages.DATA_LOADED_SUCCESS, Brushes.Green);
        }
        catch (Exception ex)
        {
            SetStatusFormatted(UiConstants.Messages.DATA_LOAD_ERROR, Brushes.Red, ex.Message);
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

            var formattedQty = UnitFormatter.Format(qty, uomCode);
            SetStatusRaw($"Добавлено: {rawItem.Name} — {formattedQty}", Brushes.Green);
        }
        else
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
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

    private async void CreateRawMaterial_Click(object sender, RoutedEventArgs e)
    {
        if (RawUomComboBox.SelectedValue is not Guid uomId || string.IsNullOrWhiteSpace(RawMaterialNameTextBox.Text))
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
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
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
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

        if (!Guid.TryParse(prodContentOrError.Replace("\"", "").Trim(), out var productId))
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

    private async void DeleteRawMaterial_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Guid id)
        {
            bool confirmed = ConfirmWindow.Show(Window.GetWindow(this), "CONFIRM_ARCHIVE_RAW_MATERIAL", "CONFIRM_ACTION_TITLE");
            if (!confirmed) return;

            await ApiService.Instance.DeleteAsync($"{CatalogConstants.RAW_MATERIALS}/{id}");
            await LoadDataAsync();
        }
    }

    private async void DeleteRecipe_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Guid id)
        {
            bool confirmed = ConfirmWindow.Show(Window.GetWindow(this), "CONFIRM_ARCHIVE_RECIPE", "CONFIRM_ACTION_TITLE");
            if (!confirmed) return;

            await ApiService.Instance.DeleteAsync($"{CatalogConstants.RECIPES}/{id}");
            await LoadDataAsync();
        }
    }

    private void SetStatus(string resourceKey, Brush color)
    {
        StatusUpdated?.Invoke(LocalizationService.Get(resourceKey), color);
    }

    private void SetStatusFormatted(string resourceKey, Brush color, params object[] args)
    {
        var format = LocalizationService.Get(resourceKey);
        StatusUpdated?.Invoke(string.Format(format, args), color);
    }

    private void SetStatusRaw(string rawText, Brush color)
    {
        StatusUpdated?.Invoke(rawText, color);
    }
}