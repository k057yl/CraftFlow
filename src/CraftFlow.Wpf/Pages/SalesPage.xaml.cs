using CraftFlow.Api.Modules.Inventory;
using CraftFlow.Api.Modules.Sales;
using CraftFlow.Api.Modules.Sales.GetStockLotDetails;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace CraftFlow.Wpf.Pages;

public sealed class InvoiceItemRow : INotifyPropertyChanged
{
    public Guid StockLotId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal AvailableQuantity { get; set; }

    private decimal _quantity;
    public decimal Quantity
    {
        get => _quantity;
        set
        {
            _quantity = Math.Min(value, AvailableQuantity);
            if (_quantity < 0) _quantity = 0;
            OnPropertyChanged(nameof(Quantity));
            OnPropertyChanged(nameof(TotalSum));
        }
    }

    private decimal _sellingPrice;
    public decimal SellingPrice
    {
        get => _sellingPrice;
        set
        {
            _sellingPrice = value < 0 ? 0 : value;
            OnPropertyChanged(nameof(SellingPrice));
            OnPropertyChanged(nameof(TotalSum));
        }
    }

    public decimal TotalSum => Quantity * SellingPrice;

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public sealed class SalesLotItemDto : INotifyPropertyChanged
{
    public Guid Id { get; set; }
    public string ItemNumber { get; set; } = string.Empty;
    public decimal CurrentWeight { get; set; }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}

public sealed class SalesStockLotDto : INotifyPropertyChanged
{
    public Guid Id { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public Guid ItemId { get; set; }
    public Guid? ProductionBatchId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public int UnitsCount { get; set; }

    public ObservableCollection<SalesLotItemDto> Items { get; set; } = [];

    public event PropertyChangedEventHandler? PropertyChanged;
}

public partial class SalesPage : Page
{
    public ObservableCollection<LookupItem> Customers { get; } = [];
    public ObservableCollection<LookupItem> Warehouses { get; } = [];
    public ObservableCollection<SalesStockLotDto> AvailableLots { get; } = [];
    public ObservableCollection<InvoiceItemRow> InvoiceItems { get; } = [];

    private const decimal DEFAULT_MARKUP_MULTIPLIER = 1.40m;
    private bool _isInitializing = true;

    public SalesPage()
    {
        InitializeComponent();

        OrderCustomerComboBox.ItemsSource = Customers;
        OrderWarehouseComboBox.ItemsSource = Warehouses;
        CustomersListBox.ItemsSource = Customers;
        AvailableLotsDataGrid.ItemsSource = AvailableLots;
        InvoiceItemsDataGrid.ItemsSource = InvoiceItems;

        InvoiceItems.CollectionChanged += (s, e) => RecalculateOrderTotals();

        Loaded += async (s, e) => await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            _isInitializing = true;

            var customers = await ApiService.Instance.GetAsync<List<LookupDto>>(SalesConstants.CUSTOMERS);
            Customers.Clear();
            customers?.ForEach(c => Customers.Add(new LookupItem(c.Id, c.Name)));

            var warehouses = await ApiService.Instance.GetAsync<List<LookupDto>>(InventoryConstants.WAREHOUSES);
            Warehouses.Clear();
            warehouses?.ForEach(w => Warehouses.Add(new LookupItem(w.Id, w.Name)));

            if (OrderCustomerComboBox.SelectedIndex < 0 && Customers.Count > 0)
                OrderCustomerComboBox.SelectedIndex = 0;

            if (OrderWarehouseComboBox.SelectedIndex < 0 && Warehouses.Count > 0)
            {
                OrderWarehouseComboBox.SelectedIndex = Warehouses.Count > 1 ? 1 : 0;
            }

            _isInitializing = false;

            SetStatus(UiConstants.Messages.DATA_LOADED_SUCCESS, Brushes.Green);

            await RefreshWarehouseStockLotsAsync();
        }
        catch (Exception ex)
        {
            _isInitializing = false;
            SetStatus($"{UiConstants.Messages.DATA_LOAD_ERROR}: {ex.Message}", Brushes.Red);
        }
    }

    private async void OrderWarehouse_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing) return;

        InvoiceItems.Clear();
        await RefreshWarehouseStockLotsAsync();
    }

    private async Task RefreshWarehouseStockLotsAsync()
    {
        if (OrderWarehouseComboBox.SelectedValue is not Guid warehouseId || warehouseId == Guid.Empty)
            return;

        try
        {
            var stockLots = await ApiService.Instance.GetAsync<List<SalesStockLotDto>>($"{InventoryConstants.STOCK_LOTS}?warehouseId={warehouseId}");

            AvailableLots.Clear();

            if (stockLots != null)
            {
                foreach (var lot in stockLots.Where(l => l.Quantity > 0))
                {
                    try
                    {
                        var details = await ApiService.Instance.GetAsync<StockLotDetailsDto>($"api/sales/stock-lots/{lot.Id}/details");
                        if (details != null && details.Items.Count > 0)
                        {
                            lot.Items = new ObservableCollection<SalesLotItemDto>(
                                details.Items.Select(i => new SalesLotItemDto
                                {
                                    Id = i.Id,
                                    ItemNumber = i.ItemNumber,
                                    CurrentWeight = i.CurrentWeight
                                })
                            );
                            lot.UnitsCount = lot.Items.Count;
                        }
                    }
                    catch { }

                    AvailableLots.Add(lot);
                }
            }
        }
        catch
        {
            AvailableLots.Clear();
        }

        RecalculateOrderTotals();
    }

    private void AvailableLotsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (AvailableLotsDataGrid.SelectedItem is SalesStockLotDto selectedLot)
        {
            AddLotToInvoice(selectedLot, selectedLot.Quantity, selectedLot.UnitsCount);
        }
    }

    private void AddSelectedUnitsToInvoice_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is SalesStockLotDto lot)
        {
            var selectedUnits = lot.Items.Where(i => i.IsSelected).ToList();

            if (selectedUnits.Count == 0)
            {
                SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
                return;
            }

            decimal selectedWeight = selectedUnits.Sum(u => u.CurrentWeight);
            int unitsCount = selectedUnits.Count;

            AddLotToInvoice(lot, selectedWeight, unitsCount);
        }
    }

    private void AddLotToInvoice(SalesStockLotDto lot, decimal weight, int unitsCount)
    {
        if (InvoiceItems.Any(i => i.StockLotId == lot.Id))
        {
            return;
        }

        decimal basePrice = lot.UnitPrice > 0 ? lot.UnitPrice * DEFAULT_MARKUP_MULTIPLIER : 450.00m;
        string displayUnits = unitsCount > 0 ? $" ({unitsCount} шт.)" : string.Empty;

        var row = new InvoiceItemRow
        {
            StockLotId = lot.Id,
            BatchNumber = $"{lot.BatchNumber}{displayUnits}",
            ProductName = string.IsNullOrWhiteSpace(lot.ProductName) ? lot.BatchNumber : lot.ProductName,
            AvailableQuantity = lot.Quantity,
            Quantity = weight,
            SellingPrice = Math.Round(basePrice, 2)
        };

        row.PropertyChanged += (s, ev) => RecalculateOrderTotals();
        InvoiceItems.Add(row);
    }

    private void RecalculateOrderTotals()
    {
        if (TotalOrderSumTextBlock == null)
            return;

        decimal totalSum = InvoiceItems.Sum(i => i.TotalSum);
        TotalOrderSumTextBlock.Text = $"${totalSum:F2}";
    }

    private async void CreateCustomer_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CustomerNameTextBox.Text))
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
            return;
        }

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(SalesConstants.CUSTOMERS, new
        {
            Name = CustomerNameTextBox.Text.Trim(),
            Phone = CustomerPhoneTextBox.Text.Trim()
        });

        if (isSuccess)
        {
            SetStatus(UiConstants.Messages.CUSTOMER_CREATED_SUCCESS, Brushes.Green);
            CustomerNameTextBox.Clear();
            CustomerPhoneTextBox.Clear();

            await LoadDataAsync();
            SalesTabControl.SelectedIndex = 0;
        }
        else
        {
            SetStatus(contentOrError, Brushes.Red);
        }
    }

    private async void ShipOrder_Click(object sender, RoutedEventArgs e)
    {
        if (OrderCustomerComboBox.SelectedValue is not Guid customerId ||
            OrderWarehouseComboBox.SelectedValue is not Guid warehouseId)
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
            return;
        }

        if (InvoiceItems.Count == 0)
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
            return;
        }

        var itemsPayload = InvoiceItems.Select(i => new
        {
            StockLotId = i.StockLotId,
            Quantity = i.Quantity,
            UnitPrice = i.SellingPrice
        }).ToList();

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(SalesConstants.ORDERS_SHIP, new
        {
            CustomerId = customerId,
            WarehouseId = warehouseId,
            Items = itemsPayload
        });

        if (isSuccess)
        {
            SetStatus($"{UiConstants.Messages.ORDER_SHIPPED_SUCCESS} ID: {contentOrError}", Brushes.Green);

            PrintInvoiceDocument(customerId, InvoiceItems.ToList(), contentOrError);

            InvoiceItems.Clear();
            await RefreshWarehouseStockLotsAsync();
        }
        else
        {
            SetStatus(contentOrError, Brushes.Red);
        }
    }

    private void PrintInvoiceDocument(Guid customerId, List<InvoiceItemRow> items, string orderId)
    {
        try
        {
            var customerName = (OrderCustomerComboBox.SelectedItem as LookupItem)?.Name ?? LocalizationService.Get("LABEL_SELECT_CUSTOMER");

            FlowDocument doc = new FlowDocument
            {
                PagePadding = new Thickness(40),
                ColumnWidth = double.PositiveInfinity
            };

            Paragraph header = new Paragraph(new Run($"{LocalizationService.Get("TITLE_SALES")} № {orderId[..Math.Min(8, orderId.Length)].ToUpper()}"))
            {
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            };
            doc.Blocks.Add(header);

            Paragraph info = new Paragraph(new Run($"{LocalizationService.Get("LABEL_PLANTED_DATE")}: {DateTime.Now:dd.MM.yyyy HH:mm}\n{LocalizationService.Get("LABEL_SELECT_CUSTOMER")}: {customerName}"))
            {
                FontSize = 12,
                Margin = new Thickness(0, 0, 0, 15)
            };
            doc.Blocks.Add(info);

            Table table = new Table { CellSpacing = 0, BorderThickness = new Thickness(1), BorderBrush = Brushes.Black };
            table.Columns.Add(new TableColumn { Width = new GridLength(220) });
            table.Columns.Add(new TableColumn { Width = new GridLength(90) });
            table.Columns.Add(new TableColumn { Width = new GridLength(90) });
            table.Columns.Add(new TableColumn { Width = new GridLength(100) });

            TableRowGroup group = new TableRowGroup();

            TableRow headerRow = new TableRow { Background = Brushes.LightGray };
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run(LocalizationService.Get("LABEL_SELECT_PRODUCT"))) { FontWeight = FontWeights.Bold }));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run(LocalizationService.Get("LABEL_QTY_KG"))) { FontWeight = FontWeights.Bold }));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run(LocalizationService.Get("LABEL_PRICE_PER_KG"))) { FontWeight = FontWeights.Bold }));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run(LocalizationService.Get("LABEL_TOTAL_ORDER_SUM"))) { FontWeight = FontWeights.Bold }));
            group.Rows.Add(headerRow);

            foreach (var item in items)
            {
                TableRow row = new TableRow();
                row.Cells.Add(new TableCell(new Paragraph(new Run($"{item.ProductName} ({item.BatchNumber})"))));
                row.Cells.Add(new TableCell(new Paragraph(new Run(item.Quantity.ToString("F2")))));
                row.Cells.Add(new TableCell(new Paragraph(new Run(item.SellingPrice.ToString("F2")))));
                row.Cells.Add(new TableCell(new Paragraph(new Run(item.TotalSum.ToString("F2")))));
                group.Rows.Add(row);
            }

            table.RowGroups.Add(group);
            doc.Blocks.Add(table);

            decimal totalSum = items.Sum(i => i.TotalSum);
            Paragraph total = new Paragraph(new Run($"\n{LocalizationService.Get("LABEL_TOTAL_ORDER_SUM")}: $ {totalSum:F2}"))
            {
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Right
            };
            doc.Blocks.Add(total);

            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                IDocumentPaginatorSource idpSource = doc;
                printDialog.PrintDocument(idpSource.DocumentPaginator, "CraftFlow Sales Invoice");
            }
        }
        catch { }
    }

    private void SetStatus(string msg, Brush color)
    {
        StatusTextBlock.Foreground = color;
        StatusTextBlock.Text = LocalizationService.Get(msg);
    }
}