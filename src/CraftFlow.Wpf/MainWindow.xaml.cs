using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Media;
using CraftFlow.SharedKernel.Constants;

namespace CraftFlow.Wpf;

public partial class MainWindow : Window
{
    private const string API_BASE_URL = "http://localhost:5109/";
    private const string TENANT_HEADER_KEY = "X-Tenant-Id";
    private const string DEFAULT_TENANT_ID = "00000000-0000-0000-0000-000000000001";

    private const string ENDPOINT_UOM = "api/catalog/units-of-measure";
    private const string ENDPOINT_RAW_MATERIALS = "api/catalog/raw-materials";
    private const string ENDPOINT_PRODUCTS = "api/catalog/products";
    private const string ENDPOINT_RECIPES = "api/catalog/recipes";
    private const string ENDPOINT_WAREHOUSES = "api/inventory/warehouses";
    private const string ENDPOINT_STOCK_LOTS = "api/inventory/stock-lots";

    private readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri(API_BASE_URL)
    };

    public ObservableCollection<LookupItem> UnitsOfMeasure { get; } = [];
    public ObservableCollection<LookupItem> RawMaterials { get; } = [];
    public ObservableCollection<LookupItem> Products { get; } = [];
    public ObservableCollection<LookupItem> Warehouses { get; } = [];

    public MainWindow()
    {
        InitializeComponent();
        _httpClient.DefaultRequestHeaders.Add(TENANT_HEADER_KEY, DEFAULT_TENANT_ID);

        RawUomComboBox.ItemsSource = UnitsOfMeasure;
        ProductUomComboBox.ItemsSource = UnitsOfMeasure;
        RecipeProductComboBox.ItemsSource = Products;
        RecipeRawMaterialComboBox.ItemsSource = RawMaterials;
        StockWarehouseComboBox.ItemsSource = Warehouses;
        StockRawMaterialComboBox.ItemsSource = RawMaterials;

        Loaded += async (s, e) => await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var uoms = await _httpClient.GetFromJsonAsync<List<LookupDto>>(ENDPOINT_UOM);
            UnitsOfMeasure.Clear();
            uoms?.ForEach(u => UnitsOfMeasure.Add(new LookupItem(u.Id, $"{u.Name} ({u.Code})")));

            var raw = await _httpClient.GetFromJsonAsync<List<LookupDto>>(ENDPOINT_RAW_MATERIALS);
            RawMaterials.Clear();
            raw?.ForEach(r => RawMaterials.Add(new LookupItem(r.Id, r.Name)));

            var prods = await _httpClient.GetFromJsonAsync<List<LookupDto>>(ENDPOINT_PRODUCTS);
            Products.Clear();
            prods?.ForEach(p => Products.Add(new LookupItem(p.Id, p.Name)));

            var warehouses = await _httpClient.GetFromJsonAsync<List<LookupDto>>(ENDPOINT_WAREHOUSES);
            Warehouses.Clear();
            warehouses?.ForEach(w => Warehouses.Add(new LookupItem(w.Id, w.Name)));

            SetStatus(ErrorCodes.UiMessages.DATA_LOADED_SUCCESS, Brushes.Green);
        }
        catch (Exception ex)
        {
            SetStatus($"{ErrorCodes.UiMessages.DATA_LOAD_ERROR}: {ex.Message}", Brushes.Red);
        }
    }

    private async void CreateUom_Click(object sender, RoutedEventArgs e)
    {
        var response = await _httpClient.PostAsJsonAsync(ENDPOINT_UOM, new
        {
            Name = UomNameTextBox.Text,
            Code = UomCodeTextBox.Text
        });

        if (response.IsSuccessStatusCode)
        {
            SetStatus(ErrorCodes.UiMessages.UOM_CREATED_SUCCESS, Brushes.Green);
            await LoadDataAsync();
        }
        else
        {
            SetStatus($"{ErrorCodes.UiMessages.API_ERROR_PREFIX}: {response.StatusCode}", Brushes.Red);
        }
    }

    private async void CreateRawMaterial_Click(object sender, RoutedEventArgs e)
    {
        if (RawUomComboBox.SelectedValue is not Guid uomId) return;

        var response = await _httpClient.PostAsJsonAsync(ENDPOINT_RAW_MATERIALS, new
        {
            Name = RawMaterialNameTextBox.Text,
            UnitOfMeasureId = uomId
        });

        if (response.IsSuccessStatusCode)
        {
            SetStatus(ErrorCodes.UiMessages.RAW_MATERIAL_CREATED_SUCCESS, Brushes.Green);
            await LoadDataAsync();
        }
        else
        {
            SetStatus($"{ErrorCodes.UiMessages.API_ERROR_PREFIX}: {response.StatusCode}", Brushes.Red);
        }
    }

    private async void CreateProduct_Click(object sender, RoutedEventArgs e)
    {
        if (ProductUomComboBox.SelectedValue is not Guid uomId) return;

        var response = await _httpClient.PostAsJsonAsync(ENDPOINT_PRODUCTS, new
        {
            Name = ProductNameTextBox.Text,
            UnitOfMeasureId = uomId
        });

        if (response.IsSuccessStatusCode)
        {
            SetStatus(ErrorCodes.UiMessages.PRODUCT_CREATED_SUCCESS, Brushes.Green);
            await LoadDataAsync();
        }
        else
        {
            SetStatus($"{ErrorCodes.UiMessages.API_ERROR_PREFIX}: {response.StatusCode}", Brushes.Red);
        }
    }

    private async void CreateRecipe_Click(object sender, RoutedEventArgs e)
    {
        if (RecipeProductComboBox.SelectedValue is not Guid productId ||
            RecipeRawMaterialComboBox.SelectedValue is not Guid rawId ||
            !decimal.TryParse(RecipeOutputQuantityTextBox.Text, out var targetOutput) ||
            !decimal.TryParse(RecipeIngredientQuantityTextBox.Text, out var ingredientQty))
        {
            SetStatus(ErrorCodes.UiMessages.INVALID_INPUT_FIELDS, Brushes.Red);
            return;
        }

        var response = await _httpClient.PostAsJsonAsync(ENDPOINT_RECIPES, new
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
            SetStatus($"{ErrorCodes.UiMessages.RECIPE_CREATED_SUCCESS} ID: {recipeId}", Brushes.Green);
        }
        else
        {
            SetStatus($"{ErrorCodes.UiMessages.API_ERROR_PREFIX}: {response.StatusCode}", Brushes.Red);
        }
    }

    private async void CreateWarehouse_Click(object sender, RoutedEventArgs e)
    {
        var response = await _httpClient.PostAsJsonAsync(ENDPOINT_WAREHOUSES, new
        {
            Name = WarehouseNameTextBox.Text,
            Address = WarehouseAddressTextBox.Text
        });

        if (response.IsSuccessStatusCode)
        {
            SetStatus(ErrorCodes.UiMessages.WAREHOUSE_CREATED_SUCCESS, Brushes.Green);
            await LoadDataAsync();
        }
        else
        {
            SetStatus($"{ErrorCodes.UiMessages.API_ERROR_PREFIX}: {response.StatusCode}", Brushes.Red);
        }
    }

    private async void AddStockLot_Click(object sender, RoutedEventArgs e)
    {
        if (StockWarehouseComboBox.SelectedValue is not Guid warehouseId ||
            StockRawMaterialComboBox.SelectedValue is not Guid rawId ||
            !decimal.TryParse(StockQuantityTextBox.Text, out var quantity))
        {
            SetStatus(ErrorCodes.UiMessages.INVALID_INPUT_FIELDS, Brushes.Red);
            return;
        }

        var response = await _httpClient.PostAsJsonAsync(ENDPOINT_STOCK_LOTS, new
        {
            WarehouseId = warehouseId,
            ItemId = rawId,
            Quantity = quantity,
            BatchNumber = StockBatchTextBox.Text
        });

        if (response.IsSuccessStatusCode)
        {
            var lotId = await response.Content.ReadFromJsonAsync<Guid>();
            SetStatus($"{ErrorCodes.UiMessages.STOCK_LOT_CREATED_SUCCESS} ID: {lotId}", Brushes.Green);
        }
        else
        {
            SetStatus($"{ErrorCodes.UiMessages.API_ERROR_PREFIX}: {response.StatusCode}", Brushes.Red);
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