using CraftFlow.Api.Modules.Inventory;
using CraftFlow.Api.Modules.Traceability;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Dtos.Common;
using CraftFlow.SharedKernel.Dtos.Traceability;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static CraftFlow.Wpf.Services.LocalizationService;

namespace CraftFlow.Wpf.Pages;

public enum TraceLinkType
{
    None,
    Customer,
    Supplier
}

public sealed class TraceTreeNode
{
    public string Icon { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string PrefixDetails { get; set; } = string.Empty;
    public string LinkText { get; set; } = string.Empty;
    public string SuffixDetails { get; set; } = string.Empty;

    public TraceLinkType LinkType { get; set; } = TraceLinkType.None;
    public Guid TargetId { get; set; }

    public bool HasLink => LinkType != TraceLinkType.None && !string.IsNullOrEmpty(LinkText);

    public ObservableCollection<TraceTreeNode> Children { get; } = [];
}

public partial class TraceabilityPage : Page
{
    public ObservableCollection<LookupItem> StockLots { get; } = [];
    public ObservableCollection<TraceTreeNode> TreeNodes { get; } = [];

    public TraceabilityPage() : this(null)
    {
    }

    public TraceabilityPage(object? dummy = null)
    {
        InitializeComponent();

        StockLotsComboBox.ItemsSource = StockLots;
        TraceTreeView.ItemsSource = TreeNodes;

        Loaded += async (s, e) => await LoadStockLotsAsync();
    }

    private async void RadioButton_Checked(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded) return;
        await LoadStockLotsAsync();
    }

    private async Task LoadStockLotsAsync()
    {
        try
        {
            StockLots.Clear();

            bool isForward = ForwardRadioButton?.IsChecked ?? true;

            string endpoint = isForward
                ? InventoryConstants.STOCK_LOTS_RAW
                : InventoryConstants.STOCK_LOTS_PRODUCTS;

            var stockLots = await ApiService.Instance.GetAsync<List<LookupDto>>(endpoint);
            stockLots?.ForEach(l => StockLots.Add(new LookupItem(l.Id, l.Name)));

            if (StockLotsComboBox.SelectedIndex < 0 && StockLots.Count > 0)
                StockLotsComboBox.SelectedIndex = 0;

            SetStatus("UI_MSG_DATA_LOADED_SUCCESS", Brushes.Green);
        }
        catch (Exception ex)
        {
            SetStatus($"{Get("UI_ERR_DATA_LOAD")}: {ex.Message}", Brushes.Red);
        }
    }

    private async void RunTraceability_Click(object sender, RoutedEventArgs e)
    {
        if (StockLotsComboBox.SelectedValue is not Guid stockLotId)
        {
            SetStatus("UI_ERR_INVALID_INPUT_FIELDS", Brushes.Red);
            return;
        }

        TreeNodes.Clear();

        if (ForwardRadioButton.IsChecked == true)
        {
            await RunForwardTraceabilityAsync(stockLotId);
        }
        else
        {
            await RunBackwardTraceabilityAsync(stockLotId);
        }
    }

    private async Task RunForwardTraceabilityAsync(Guid stockLotId)
    {
        try
        {
            var endpoint = $"{TraceabilityConstants.TRACEABILITY_FORWARD}/{stockLotId}";
            var traceData = await ApiService.Instance.GetAsync<ForwardTraceabilityDto>(endpoint);

            if (traceData != null && !string.IsNullOrEmpty(traceData.RawMaterialName))
            {
                var rootNode = new TraceTreeNode
                {
                    Icon = "📦",
                    Title = traceData.RawMaterialName,
                    PrefixDetails = $"{Get("UI_LABEL_RAW_LOT")}: {traceData.RawMaterialBatchNumber}"
                };

                foreach (var batch in traceData.Batches ?? [])
                {
                    var batchIdStr = batch.ProductionBatchId.ToString();
                    var shortBatchId = batchIdStr.Length >= 8 ? batchIdStr[..8].ToUpperInvariant() : batchIdStr;

                    var batchNode = new TraceTreeNode
                    {
                        Icon = "🧀",
                        Title = $"{Get("UI_LABEL_BATCH_COOK")} #{shortBatchId}",
                        PrefixDetails = $"{Get("UI_LABEL_STARTED_AT")}: {batch.StartedAt:dd.MM.yyyy HH:mm}"
                    };

                    rootNode.Children.Add(batchNode);
                }

                TreeNodes.Add(rootNode);
                SetStatus("UI_MSG_DATA_LOADED_SUCCESS", Brushes.Green);
            }
            else
            {
                SetStatus("UI_MSG_TRACE_NOT_FOUND", Brushes.OrangeRed);
            }
        }
        catch (Exception ex)
        {
            SetStatus($"{Get("UI_ERR_DATA_LOAD")}: {ex.Message}", Brushes.Red);
        }
    }

    private async Task RunBackwardTraceabilityAsync(Guid productStockLotId)
    {
        try
        {
            var endpoint = $"{TraceabilityConstants.TRACEABILITY_BACKWARD}/{productStockLotId}";
            var traceData = await ApiService.Instance.GetAsync<BackwardTraceabilityDto>(endpoint);

            if (traceData != null)
            {
                bool hasCustomer = !string.IsNullOrEmpty(traceData.CustomerName) &&
                                  traceData.CustomerName != FormattingConstants.CONST_DEFAULT_CUSTOMER_NAME;

                var defaultCustomerText = Get("UI_LABEL_IN_STOCK");

                var rootNode = new TraceTreeNode
                {
                    Icon = FormattingConstants.ICON_FINISHED_PRODUCT,
                    Title = traceData.ProductName,
                    PrefixDetails = $"{Get("UI_LABEL_LOT")}: {traceData.ProductBatchNumber} | {Get("UI_LABEL_STOCK_REMAIN")}: {traceData.CurrentStockQuantity:N2} {Get("UI_UNIT_KG")} | {Get("UI_LABEL_CUSTOMER")}: ",
                    LinkText = hasCustomer ? traceData.CustomerName : defaultCustomerText,
                    LinkType = hasCustomer && traceData.CustomerId.HasValue ? TraceLinkType.Customer : TraceLinkType.None,
                    TargetId = traceData.CustomerId ?? Guid.Empty
                };

                TraceTreeNode parentForBatch = rootNode;

                bool hasRealAging = traceData.AgingDaysTotal > 0 ||
                    (!string.IsNullOrWhiteSpace(traceData.StorageChamberName)
                     && traceData.StorageChamberName != FormattingConstants.CONST_DEFAULT_CHAMBER_NAME
                     && traceData.StorageChamberName != "Камера");

                if (hasRealAging)
                {
                    var agingLossInfo = traceData.AgingLossPercentage > 0
                        ? $" | {Get("UI_LABEL_SHRINKAGE")}: {traceData.AgingLossPercentage:N2}%"
                        : string.Empty;

                    var agingNode = new TraceTreeNode
                    {
                        Icon = "⏳",
                        Title = $"{Get("UI_LABEL_AGING")}: {traceData.StorageChamberName}",
                        PrefixDetails = $"{Get("UI_LABEL_DAYS_IN_CHAMBER")}: {traceData.AgingDaysTotal}{agingLossInfo}"
                    };

                    rootNode.Children.Add(agingNode);
                    parentForBatch = agingNode;
                }

                if (traceData.OriginBatch != null)
                {
                    var batchIdStr = traceData.OriginBatch.ProductionBatchId.ToString();
                    var shortBatchId = batchIdStr.Length >= 8 ? batchIdStr[..8].ToUpperInvariant() : batchIdStr;

                    var yieldInfo = traceData.OriginBatch.OutputYieldPercentage > 0
                        ? $" | {Get("UI_LABEL_YIELD")}: {traceData.OriginBatch.ActualOutputQuantity:N2} {Get("UI_UNIT_KG")} ({traceData.OriginBatch.OutputYieldPercentage:N1}% {Get("UI_LABEL_OF_PLAN")})"
                        : string.Empty;

                    var batchNode = new TraceTreeNode
                    {
                        Icon = FormattingConstants.ICON_PRODUCTION_BATCH,
                        Title = $"{Get("UI_LABEL_BATCH_COOK")} #{shortBatchId}",
                        PrefixDetails = $"{Get("UI_LABEL_STARTED_AT")}: {traceData.OriginBatch.StartedAt:dd.MM.yyyy HH:mm}{yieldInfo}"
                    };

                    foreach (var ing in traceData.OriginBatch.ConsumedIngredients ?? [])
                    {
                        bool hasSupplier = !string.IsNullOrEmpty(ing.SupplierName) && ing.SupplierName != "—" && ing.SupplierName != FormattingConstants.NOT_AVAILABLE;
                        var uom = !string.IsNullOrEmpty(ing.UnitOfMeasure) ? $" {ing.UnitOfMeasure}" : string.Empty;

                        batchNode.Children.Add(new TraceTreeNode
                        {
                            Icon = FormattingConstants.ICON_RAW_MATERIAL,
                            Title = ing.RawMaterialName,
                            PrefixDetails = $"{Get("UI_LABEL_LOT")}: {ing.BatchNumber} | {Get("UI_LABEL_CONSUMPTION")}: {ing.QuantityUsed:N2}{uom}" + (hasSupplier ? $" | {Get("UI_LABEL_SUPPLIER")}: " : string.Empty),
                            LinkText = hasSupplier ? ing.SupplierName : string.Empty,
                            LinkType = hasSupplier && ing.SupplierId.HasValue ? TraceLinkType.Supplier : TraceLinkType.None,
                            TargetId = ing.SupplierId ?? Guid.Empty
                        });
                    }

                    parentForBatch.Children.Add(batchNode);
                }

                TreeNodes.Add(rootNode);
                SetStatus("UI_MSG_DATA_LOADED_SUCCESS", Brushes.Green);
            }
            else
            {
                SetStatus("UI_MSG_TRACE_NOT_FOUND", Brushes.OrangeRed);
            }
        }
        catch (Exception ex)
        {
            SetStatus($"{Get("UI_ERR_DATA_LOAD")}: {ex.Message}", Brushes.Red);
        }
    }

    private void EntityLink_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Documents.Hyperlink link && link.DataContext is TraceTreeNode node)
        {
            string entityType = node.LinkType == TraceLinkType.Customer ? Get("UI_ENTITY_CUSTOMER") : Get("UI_ENTITY_SUPPLIER");
            var msgFormat = Get("UI_MSG_CARD_IN_DEVELOPMENT_FMT");
            MessageBox.Show(string.Format(msgFormat, entityType, node.LinkText, node.TargetId), Get("UI_CAPTION_NAVIGATION"), MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void SetStatus(string msgKeyOrText, Brush color)
    {
        StatusTextBlock.Foreground = color;
        StatusTextBlock.Text = Get(msgKeyOrText);
    }
}