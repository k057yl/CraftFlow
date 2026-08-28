using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Services;

namespace CraftFlow.Wpf.Pages;

public record ProductStockInfoDto(
    decimal Quantity,
    decimal UnitCost,
    decimal BasePrice
);

public partial class SalesPage : Page
{
    public ObservableCollection<LookupItem> Customers { get; } = [];
    public ObservableCollection<LookupItem> Warehouses { get; } = [];
    public ObservableCollection<LookupItem> Products { get; } = [];

    private bool _isAutoDiscountApplied = false;

    public SalesPage()
    {
        InitializeComponent();

        OrderCustomerComboBox.ItemsSource = Customers;
        OrderWarehouseComboBox.ItemsSource = Warehouses;
        OrderProductComboBox.ItemsSource = Products;

        Loaded += async (s, e) => await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var customers = await ApiService.Instance.GetAsync<List<LookupDto>>(Endpoints.CUSTOMERS);
            Customers.Clear();
            customers?.ForEach(c => Customers.Add(new LookupItem(c.Id, c.Name)));

            var warehouses = await ApiService.Instance.GetAsync<List<LookupDto>>(Endpoints.WAREHOUSES);
            Warehouses.Clear();
            warehouses?.ForEach(w => Warehouses.Add(new LookupItem(w.Id, w.Name)));

            var prods = await ApiService.Instance.GetAsync<List<LookupDto>>(Endpoints.PRODUCTS);
            Products.Clear();
            prods?.ForEach(p => Products.Add(new LookupItem(p.Id, p.Name)));

            if (OrderWarehouseComboBox.SelectedIndex < 0 && Warehouses.Count > 0)
                OrderWarehouseComboBox.SelectedIndex = Warehouses.Count > 1 ? 1 : 0;

            if (OrderProductComboBox.SelectedIndex < 0 && Products.Count > 0)
                OrderProductComboBox.SelectedIndex = 0;

            if (OrderCustomerComboBox.SelectedIndex < 0 && Customers.Count > 0)
                OrderCustomerComboBox.SelectedIndex = 0;

            SetStatus(UiConstants.Messages.DATA_LOADED_SUCCESS, Brushes.Green);

            await RefreshProductStockAsync();
        }
        catch (Exception ex)
        {
            SetStatus($"{UiConstants.Messages.DATA_LOAD_ERROR}: {ex.Message}", Brushes.Red);
        }
    }

    private async void OrderWarehouse_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        await RefreshProductStockAsync();
    }

    private async void OrderProduct_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        await RefreshProductStockAsync();
    }

    private async Task RefreshProductStockAsync()
    {
        if (OrderWarehouseComboBox.SelectedValue is Guid warehouseId &&
            OrderProductComboBox.SelectedValue is Guid productId)
        {
            try
            {
                var info = await ApiService.Instance.GetAsync<ProductStockInfoDto>(
                    $"api/sales/stock-info?warehouseId={warehouseId}&productId={productId}");

                if (info != null)
                {
                    ProductStockTextBlock.Text = $"{info.Quantity:F2}";
                    ProductCostTextBlock.Text = $"${info.UnitCost:F2}";

                    if (info.BasePrice > 0)
                    {
                        OrderBasePriceTextBox.Text = info.BasePrice.ToString("F2", CultureInfo.InvariantCulture);
                    }
                }
            }
            catch
            {
                ProductStockTextBlock.Text = "0.00";
                ProductCostTextBlock.Text = "$0.00";
            }
        }

        RecalculateOrderTotals();
    }

    private void CalculationInputs_Changed(object sender, TextChangedEventArgs e)
    {
        RecalculateOrderTotals();
    }

    private void RecalculateOrderTotals()
    {
        if (OrderQuantityTextBox == null || OrderBasePriceTextBox == null ||
            OrderDiscountTextBox == null || FinalUnitPriceTextBlock == null || TotalOrderSumTextBlock == null)
            return;

        var rawQtyText = OrderQuantityTextBox.Text.Replace(',', '.');
        var rawPriceText = OrderBasePriceTextBox.Text.Replace(',', '.');
        var rawDiscountText = OrderDiscountTextBox.Text.Replace(',', '.');

        decimal.TryParse(rawQtyText, NumberStyles.Any, CultureInfo.InvariantCulture, out var quantity);
        decimal.TryParse(rawPriceText, NumberStyles.Any, CultureInfo.InvariantCulture, out var basePrice);
        decimal.TryParse(rawDiscountText, NumberStyles.Any, CultureInfo.InvariantCulture, out var discountPercent);

        if (quantity >= 20)
        {
            if (discountPercent < 10) discountPercent = 10;
        }
        else if (quantity >= 10)
        {
            if (discountPercent < 5) discountPercent = 5;
        }

        var discountedUnitPrice = basePrice * (1m - (discountPercent / 100m));
        if (discountedUnitPrice < 0) discountedUnitPrice = 0;

        var totalSum = quantity * discountedUnitPrice;

        FinalUnitPriceTextBlock.Text = $"${discountedUnitPrice:F2}";
        TotalOrderSumTextBlock.Text = $"${totalSum:F2}";
    }

    private async void CreateCustomer_Click(object sender, RoutedEventArgs e)
    {
        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(Endpoints.CUSTOMERS, new
        {
            Name = CustomerNameTextBox.Text,
            Phone = CustomerPhoneTextBox.Text
        });

        if (isSuccess)
        {
            SetStatus(UiConstants.Messages.CUSTOMER_CREATED_SUCCESS, Brushes.Green);
            await LoadDataAsync();
        }
        else
        {
            SetStatus(contentOrError, Brushes.Red);
        }
    }

    private async void ShipOrder_Click(object sender, RoutedEventArgs e)
    {
        if (OrderCustomerComboBox.SelectedValue is not Guid customerId ||
            OrderWarehouseComboBox.SelectedValue is not Guid warehouseId ||
            OrderProductComboBox.SelectedValue is not Guid productId)
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
            return;
        }

        var rawQtyText = OrderQuantityTextBox.Text.Replace(',', '.');
        var rawPriceText = OrderBasePriceTextBox.Text.Replace(',', '.');
        var rawDiscountText = OrderDiscountTextBox.Text.Replace(',', '.');

        if (!decimal.TryParse(rawQtyText, NumberStyles.Any, CultureInfo.InvariantCulture, out var quantity) ||
            !decimal.TryParse(rawPriceText, NumberStyles.Any, CultureInfo.InvariantCulture, out var basePrice) ||
            !decimal.TryParse(rawDiscountText, NumberStyles.Any, CultureInfo.InvariantCulture, out var discountPercent))
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
            return;
        }

        var finalUnitPrice = basePrice * (1m - (discountPercent / 100m));

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(Endpoints.ORDERS_SHIP, new
        {
            CustomerId = customerId,
            WarehouseId = warehouseId,
            Items = new[]
            {
                new { ProductId = productId, Quantity = quantity, UnitPrice = finalUnitPrice }
            }
        });

        if (isSuccess)
        {
            SetStatus($"{UiConstants.Messages.ORDER_SHIPPED_SUCCESS} ORDER_ID: {contentOrError}", Brushes.Green);
            await LoadDataAsync();
            await RefreshProductStockAsync();
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