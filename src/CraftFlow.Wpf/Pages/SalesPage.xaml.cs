using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Services;

namespace CraftFlow.Wpf.Pages;

public partial class SalesPage : Page
{
    public ObservableCollection<LookupItem> Customers { get; } = [];
    public ObservableCollection<LookupItem> Warehouses { get; } = [];
    public ObservableCollection<LookupItem> Products { get; } = [];

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

            SetStatus(UiConstants.Messages.DATA_LOADED_SUCCESS, Brushes.Green);
        }
        catch (Exception ex)
        {
            SetStatus($"{UiConstants.Messages.DATA_LOAD_ERROR}: {ex.Message}", Brushes.Red);
        }
    }

    private async void CreateCustomer_Click(object sender, RoutedEventArgs e)
    {
        var response = await ApiService.Instance.PostAsync(Endpoints.CUSTOMERS, new
        {
            Name = CustomerNameTextBox.Text,
            Phone = CustomerPhoneTextBox.Text
        });

        if (response.IsSuccessStatusCode)
        {
            SetStatus(UiConstants.Messages.CUSTOMER_CREATED_SUCCESS, Brushes.Green);
            await LoadDataAsync();
        }
        else
        {
            SetStatus($"{UiConstants.Messages.API_ERROR_PREFIX}: {response.StatusCode}", Brushes.Red);
        }
    }

    private async void ShipOrder_Click(object sender, RoutedEventArgs e)
    {
        if (OrderCustomerComboBox.SelectedValue is not Guid customerId ||
            OrderWarehouseComboBox.SelectedValue is not Guid warehouseId ||
            OrderProductComboBox.SelectedValue is not Guid productId ||
            !decimal.TryParse(OrderQuantityTextBox.Text, out var quantity) ||
            !decimal.TryParse(OrderPriceTextBox.Text, out var price))
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
            return;
        }

        var response = await ApiService.Instance.PostAsync(Endpoints.ORDERS_SHIP, new
        {
            CustomerId = customerId,
            WarehouseId = warehouseId,
            Items = new[]
            {
                new { ProductId = productId, Quantity = quantity, UnitPrice = price }
            }
        });

        if (response.IsSuccessStatusCode)
        {
            var orderId = await response.Content.ReadFromJsonAsync<Guid>();
            SetStatus($"{UiConstants.Messages.ORDER_SHIPPED_SUCCESS} ORDER_ID: {orderId}", Brushes.Green);
            await LoadDataAsync();
        }
        else
        {
            SetStatus($"{UiConstants.Messages.API_ERROR_PREFIX}: {response.StatusCode}", Brushes.Red);
        }
    }

    private void SetStatus(string msg, Brush color)
    {
        StatusTextBlock.Foreground = color;
        StatusTextBlock.Text = msg;
    }
}