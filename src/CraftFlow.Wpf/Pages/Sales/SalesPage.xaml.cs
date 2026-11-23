using CraftFlow.SharedKernel.Constants.Endpoints;
using CraftFlow.SharedKernel.Dtos.Catalog.Inventory;
using CraftFlow.SharedKernel.Dtos.Common;
using CraftFlow.SharedKernel.Dtos.Sale;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Models.Sales;
using CraftFlow.Wpf.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using static CraftFlow.Wpf.Services.LocalizationService;

namespace CraftFlow.Wpf.Pages.Sales;

public partial class SalesPage : Page
{
    public ObservableCollection<LookupItem> Customers { get; } = [];
    public ObservableCollection<LookupItem> Warehouses { get; } = [];
    public ObservableCollection<SalesStockLotViewItem> AvailableLots { get; } = [];
    public ObservableCollection<InvoiceItemRow> InvoiceItems { get; } = [];

    private const decimal DEFAULT_MARKUP_MULTIPLIER = 1.40m;
    private bool _isInitializing = true;

    public SalesPage() : this(null)
    {
    }

    public SalesPage(object? dummy = null)
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

            SetStatus("UI_MSG_DATA_LOADED_SUCCESS", Brushes.Green);

            await RefreshWarehouseStockLotsAsync();
        }
        catch (Exception ex)
        {
            _isInitializing = false;
            SetStatusRaw($"{Get("UI_ERR_DATA_LOAD")}: {ex.Message}", Brushes.Red);
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
            var stockLots = await ApiService.Instance.GetAsync<List<StockLotDto>>($"{InventoryConstants.STOCK_LOTS}?warehouseId={warehouseId}");

            AvailableLots.Clear();

            if (stockLots != null)
            {
                foreach (var dto in stockLots.Where(l => l.Quantity > 0))
                {
                    var viewItem = new SalesStockLotViewItem
                    {
                        Id = dto.Id,
                        BatchNumber = dto.BatchNumber ?? "—",
                        ProductName = dto.ItemName,
                        Quantity = dto.Quantity,
                        UnitPrice = dto.UnitPrice,
                        UnitsCount = 0
                    };

                    try
                    {
                        var details = await ApiService.Instance.GetAsync<StockLotDetailsDto>($"api/sales/stock-lots/{dto.Id}/details");
                        if (details != null)
                        {
                            viewItem.UnitsCount = details.UnitsCount;

                            if (details.Items.Count > 0)
                            {
                                viewItem.Items = new ObservableCollection<SalesLotItemViewItem>(
                                    details.Items.Select(i => new SalesLotItemViewItem
                                    {
                                        Id = i.Id,
                                        ItemNumber = i.ItemNumber,
                                        CurrentWeight = i.CurrentWeight
                                    })
                                );
                                viewItem.UnitsCount = viewItem.Items.Count;
                            }
                        }
                    }
                    catch
                    {
                    }

                    AvailableLots.Add(viewItem);
                }
            }
        }
        catch (Exception ex)
        {
            AvailableLots.Clear();
            SetStatusRaw($"{Get("UI_ERR_DATA_LOAD")}: {ex.Message}", Brushes.Red);
        }

        RecalculateOrderTotals();
    }

    private void AvailableLotsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (AvailableLotsDataGrid.SelectedItem is SalesStockLotViewItem selectedLot)
        {
            AddFullLotToInvoice(selectedLot);
        }
    }

    private void AddLotButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is SalesStockLotViewItem selectedLot)
        {
            AddFullLotToInvoice(selectedLot);
        }
    }

    private void AddFullLotToInvoice(SalesStockLotViewItem lot)
    {
        if (lot.Items.Count > 0)
        {
            var allIds = lot.Items.Select(i => i.Id).ToList();
            decimal totalWeight = lot.Items.Sum(i => i.CurrentWeight);
            AddLotToInvoice(lot, totalWeight, allIds.Count, allIds);
        }
        else
        {
            AddLotToInvoice(lot, lot.Quantity, lot.UnitsCount, []);
        }
    }

    private void AddSelectedUnitsToInvoice_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is SalesStockLotViewItem lot)
        {
            if (lot.Items.Count == 0)
            {
                AddFullLotToInvoice(lot);
                return;
            }

            var selectedUnits = lot.Items.Where(i => i.IsSelected).ToList();

            if (selectedUnits.Count == 0)
            {
                SetStatus("UI_ERR_INVALID_INPUT_FIELDS", Brushes.Red);
                return;
            }

            decimal selectedWeight = selectedUnits.Sum(u => u.CurrentWeight);
            var selectedIds = selectedUnits.Select(u => u.Id).ToList();

            AddLotToInvoice(lot, selectedWeight, selectedUnits.Count, selectedIds);
        }
    }

    private void AddLotToInvoice(SalesStockLotViewItem lot, decimal weight, int unitsCount, List<Guid> selectedItemIds)
    {
        string unitsSuffix = Get("UI_LABEL_PCS_SUFFIX");
        string displayUnits = unitsCount > 0 ? $" ({unitsCount} {unitsSuffix})" : string.Empty;
        string displayBatch = $"{lot.BatchNumber}{displayUnits}";

        var existing = InvoiceItems.FirstOrDefault(i => i.StockLotId == lot.Id);
        if (existing != null)
        {
            existing.BatchNumber = displayBatch;
            existing.Quantity = weight;
            existing.SelectedItemIds = selectedItemIds;
            InvoiceItemsDataGrid.Items.Refresh();
            RecalculateOrderTotals();
            return;
        }

        decimal basePrice = lot.UnitPrice > 0 ? lot.UnitPrice * DEFAULT_MARKUP_MULTIPLIER : 450.00m;

        var row = new InvoiceItemRow
        {
            StockLotId = lot.Id,
            BatchNumber = displayBatch,
            ProductName = string.IsNullOrWhiteSpace(lot.ProductName) ? lot.BatchNumber : lot.ProductName,
            AvailableQuantity = lot.Quantity,
            Quantity = weight,
            SellingPrice = Math.Round(basePrice, 2),
            SelectedItemIds = selectedItemIds
        };

        row.PropertyChanged += (s, ev) => RecalculateOrderTotals();
        InvoiceItems.Add(row);
    }

    private void RemoveInvoiceItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is InvoiceItemRow row)
        {
            InvoiceItems.Remove(row);
        }
    }

    private void InvoiceItemsDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        Dispatcher.BeginInvoke(RecalculateOrderTotals);
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
            SetStatus("UI_ERR_INVALID_INPUT_FIELDS", Brushes.Red);
            return;
        }

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(SalesConstants.CUSTOMERS, new
        {
            Name = CustomerNameTextBox.Text.Trim(),
            Phone = CustomerPhoneTextBox.Text.Trim()
        });

        if (isSuccess)
        {
            SetStatus("UI_MSG_CUSTOMER_CREATED_SUCCESS", Brushes.Green);
            CustomerNameTextBox.Clear();
            CustomerPhoneTextBox.Clear();

            await LoadDataAsync();
            SalesTabControl.SelectedIndex = 0;
        }
        else
        {
            SetStatusRaw(contentOrError, Brushes.Red);
        }
    }

    private async void ShipOrder_Click(object sender, RoutedEventArgs e)
    {
        if (OrderCustomerComboBox.SelectedValue is not Guid customerId ||
            OrderWarehouseComboBox.SelectedValue is not Guid warehouseId)
        {
            SetStatus("UI_ERR_INVALID_INPUT_FIELDS", Brushes.Red);
            return;
        }

        if (InvoiceItems.Count == 0)
        {
            SetStatus("UI_ERR_INVALID_INPUT_FIELDS", Brushes.Red);
            return;
        }

        foreach (var item in InvoiceItems)
        {
            if (item.Quantity <= 0 || item.Quantity > item.AvailableQuantity)
            {
                SetStatusRaw($"Партия «{item.BatchNumber}»: количество к отгрузке ({item.Quantity:F2}) должно быть больше 0 и не превышать остаток ({item.AvailableQuantity:F2}).", Brushes.Red);
                return;
            }
        }

        var itemsPayload = InvoiceItems.Select(i => new
        {
            StockLotId = i.StockLotId,
            Quantity = i.Quantity,
            UnitPrice = i.SellingPrice,
            StockLotItemIds = i.SelectedItemIds.Count > 0 ? i.SelectedItemIds : null
        }).ToList();

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(SalesConstants.ORDERS_SHIP, new
        {
            CustomerId = customerId,
            WarehouseId = warehouseId,
            Items = itemsPayload
        });

        if (isSuccess)
        {
            var msgFormat = Get("UI_MSG_ORDER_SHIPPED_SUCCESS_FMT");
            SetStatusRaw(string.Format(msgFormat, contentOrError), Brushes.Green);

            PrintInvoiceDocument(customerId, InvoiceItems.ToList(), contentOrError);

            InvoiceItems.Clear();
            await RefreshWarehouseStockLotsAsync();
        }
        else
        {
            SetStatusRaw(contentOrError, Brushes.Red);
        }
    }

    private void PrintInvoiceDocument(Guid customerId, List<InvoiceItemRow> items, string orderId)
    {
        try
        {
            var customerName = (OrderCustomerComboBox.SelectedItem as LookupItem)?.Name ?? Get("LABEL_SELECT_CUSTOMER");

            FlowDocument doc = new FlowDocument
            {
                PagePadding = new Thickness(40),
                ColumnWidth = double.PositiveInfinity
            };

            Paragraph header = new Paragraph(new Run($"{Get("TITLE_SALES")} № {orderId[..Math.Min(8, orderId.Length)].ToUpper()}"))
            {
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            };
            doc.Blocks.Add(header);

            Paragraph info = new Paragraph(new Run($"{Get("LABEL_PLANTED_DATE")}: {DateTime.Now:dd.MM.yyyy HH:mm}\n{Get("LABEL_SELECT_CUSTOMER")}: {customerName}"))
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
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run(Get("LABEL_SELECT_PRODUCT"))) { FontWeight = FontWeights.Bold }));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run(Get("LABEL_QTY_KG"))) { FontWeight = FontWeights.Bold }));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run(Get("LABEL_PRICE_PER_KG"))) { FontWeight = FontWeights.Bold }));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run(Get("LABEL_TOTAL_ORDER_SUM"))) { FontWeight = FontWeights.Bold }));
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
            Paragraph total = new Paragraph(new Run($"\n{Get("LABEL_TOTAL_ORDER_SUM")}: $ {totalSum:F2}"))
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

    private void SetStatus(string msgKey, Brush color)
    {
        StatusTextBlock.Foreground = color;
        StatusTextBlock.Text = Get(msgKey);
    }

    private void SetStatusRaw(string text, Brush color)
    {
        StatusTextBlock.Foreground = color;
        StatusTextBlock.Text = text;
    }
}