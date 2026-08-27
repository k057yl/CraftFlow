using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Media;

namespace CraftFlow.Wpf;

public partial class MainWindow : Window
{
    private readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri("http://localhost:5109/")
    };

    public ObservableCollection<LookupItem> UnitsOfMeasure { get; } = [];
    public ObservableCollection<LookupItem> RawMaterials { get; } = [];
    public ObservableCollection<LookupItem> Products { get; } = [];

    public MainWindow()
    {
        InitializeComponent();
        _httpClient.DefaultRequestHeaders.Add("X-Tenant-Id", "00000000-0000-0000-0000-000000000001");

        RawUomComboBox.ItemsSource = UnitsOfMeasure;
        ProductUomComboBox.ItemsSource = UnitsOfMeasure;
        RecipeProductComboBox.ItemsSource = Products;
        RecipeRawMaterialComboBox.ItemsSource = RawMaterials;

        Loaded += async (s, e) => await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var uoms = await _httpClient.GetFromJsonAsync<List<LookupDto>>("api/catalog/units-of-measure");
            UnitsOfMeasure.Clear();
            uoms?.ForEach(u => UnitsOfMeasure.Add(new LookupItem(u.Id, $"{u.Name} ({u.Code})")));

            var raw = await _httpClient.GetFromJsonAsync<List<LookupDto>>("api/catalog/raw-materials");
            RawMaterials.Clear();
            raw?.ForEach(r => RawMaterials.Add(new LookupItem(r.Id, r.Name)));

            var prods = await _httpClient.GetFromJsonAsync<List<LookupDto>>("api/catalog/products");
            Products.Clear();
            prods?.ForEach(p => Products.Add(new LookupItem(p.Id, p.Name)));

            SetStatus("Данные загружены из базы Postgres.", Brushes.Green);
        }
        catch (Exception ex)
        {
            SetStatus($"Ошибка загрузки данных: {ex.Message}", Brushes.Red);
        }
    }

    private async void CreateUom_Click(object sender, RoutedEventArgs e)
    {
        var response = await _httpClient.PostAsJsonAsync("api/catalog/units-of-measure", new
        {
            Name = UomNameTextBox.Text,
            Code = UomCodeTextBox.Text
        });

        if (response.IsSuccessStatusCode)
        {
            SetStatus("ЕИ создана!", Brushes.Green);
            await LoadDataAsync();
        }
    }

    private async void CreateRawMaterial_Click(object sender, RoutedEventArgs e)
    {
        if (RawUomComboBox.SelectedValue is not Guid uomId) return;

        var response = await _httpClient.PostAsJsonAsync("api/catalog/raw-materials", new
        {
            Name = RawMaterialNameTextBox.Text,
            UnitOfMeasureId = uomId
        });

        if (response.IsSuccessStatusCode)
        {
            SetStatus("Сырье создано!", Brushes.Green);
            await LoadDataAsync();
        }
    }

    private async void CreateProduct_Click(object sender, RoutedEventArgs e)
    {
        if (ProductUomComboBox.SelectedValue is not Guid uomId) return;

        var response = await _httpClient.PostAsJsonAsync("api/catalog/products", new
        {
            Name = ProductNameTextBox.Text,
            UnitOfMeasureId = uomId
        });

        if (response.IsSuccessStatusCode)
        {
            SetStatus("Продукт создан!", Brushes.Green);
            await LoadDataAsync();
        }
    }

    private async void CreateRecipe_Click(object sender, RoutedEventArgs e)
    {
        if (RecipeProductComboBox.SelectedValue is not Guid productId ||
            RecipeRawMaterialComboBox.SelectedValue is not Guid rawId ||
            !decimal.TryParse(RecipeOutputQuantityTextBox.Text, out var targetOutput) ||
            !decimal.TryParse(RecipeIngredientQuantityTextBox.Text, out var ingredientQty))
        {
            SetStatus("Проверь выбранный продукт, сырье и числовые поля!", Brushes.Red);
            return;
        }

        var response = await _httpClient.PostAsJsonAsync("api/catalog/recipes", new
        {
            ProductId = productId,
            Name = RecipeNameTextBox.Text,
            TargetOutputQuantity = targetOutput,
            Ingredients = new[]
            {
                new { RawMaterialId = rawId, Quantity = ingredientQty }
            }
        });

        if (response.IsSuccessStatusCode)
        {
            var recipeId = await response.Content.ReadFromJsonAsync<Guid>();
            SetStatus($"Рецепт успешно создан! ID: {recipeId}", Brushes.Green);
        }
        else
        {
            SetStatus($"Ошибка API: {response.StatusCode}", Brushes.Red);
        }
    }

    private void SetStatus(string msg, Brush color)
    {
        StatusTextBlock.Foreground = color;
        StatusTextBlock.Text = msg;
    }
}

public record LookupItem(Guid Id, string Name);
public record LookupDto(Guid Id, string Name, string? Code);