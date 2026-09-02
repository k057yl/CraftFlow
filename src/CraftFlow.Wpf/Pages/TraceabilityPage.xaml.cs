using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Services;

namespace CraftFlow.Wpf.Pages;

public record ForwardTraceabilityDto(
    Guid RawMaterialStockLotId,
    string RawMaterialBatchNumber,
    string RawMaterialName,
    List<TraceabilityProductionBatchDto> Batches
);

public record BackwardTraceabilityDto(
    Guid? SalesOrderId,
    string CustomerName,
    Guid ProductStockLotId,
    string ProductBatchNumber,
    string ProductName,
    decimal CurrentStockQuantity,
    decimal UnitPrice,
    int AgingDaysTotal,
    decimal AgingLossPercentage,
    string StorageChamberName,
    TraceabilityProductionBatchDto OriginBatch
);

public record TraceabilityProductionBatchDto(
    Guid ProductionBatchId,
    string BatchStatus,
    DateTime StartedAt,
    DateTime? CompletedAt,
    decimal PlannedQuantity,
    decimal ActualOutputQuantity,
    decimal OutputYieldPercentage,
    List<TraceabilityAgingLotDto> AgingLots,
    List<TraceabilityIngredientDto> ConsumedIngredients
);

public record TraceabilityAgingLotDto(
    Guid AgingLotId,
    string AgingBatchNumber,
    string ChamberName,
    string AgingStatus
);

public record TraceabilityIngredientDto(
    Guid RawMaterialStockLotId,
    string RawMaterialName,
    string BatchNumber,
    decimal QuantityUsed,
    string UnitOfMeasure,
    string SupplierName
);

public sealed class TraceTreeNode
{
    public string Icon { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public ObservableCollection<TraceTreeNode> Children { get; } = [];
}

public partial class TraceabilityPage : Page
{
    public ObservableCollection<LookupItem> StockLots { get; } = [];
    public ObservableCollection<TraceTreeNode> TreeNodes { get; } = [];

    public TraceabilityPage()
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
                ? "api/inventory/stock-lots/raw"
                : "api/inventory/stock-lots/products";

            var stockLots = await ApiService.Instance.GetAsync<List<LookupDto>>(endpoint);
            stockLots?.ForEach(l => StockLots.Add(new LookupItem(l.Id, l.Name)));

            if (StockLotsComboBox.SelectedIndex < 0 && StockLots.Count > 0)
                StockLotsComboBox.SelectedIndex = 0;

            SetStatus(UiConstants.Messages.DATA_LOADED_SUCCESS, Brushes.Green);
        }
        catch (Exception ex)
        {
            SetStatus($"{UiConstants.Messages.DATA_LOAD_ERROR}: {ex.Message}", Brushes.Red);
        }
    }

    private async void RunTraceability_Click(object sender, RoutedEventArgs e)
    {
        if (StockLotsComboBox.SelectedValue is not Guid stockLotId)
        {
            SetStatus(UiConstants.Messages.INVALID_INPUT_FIELDS, Brushes.Red);
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
            var endpoint = $"{Endpoints.TRACEABILITY_FORWARD}/{stockLotId}";
            var traceData = await ApiService.Instance.GetAsync<ForwardTraceabilityDto>(endpoint);

            if (traceData != null && !string.IsNullOrEmpty(traceData.RawMaterialName))
            {
                var rootNode = new TraceTreeNode
                {
                    Icon = "📦",
                    Title = traceData.RawMaterialName,
                    Details = $"Партия сырья: {traceData.RawMaterialBatchNumber}"
                };

                foreach (var batch in traceData.Batches ?? [])
                {
                    var batchIdStr = batch.ProductionBatchId.ToString();
                    var shortBatchId = batchIdStr.Length >= 8 ? batchIdStr[..8].ToUpperInvariant() : batchIdStr;

                    var batchNode = new TraceTreeNode
                    {
                        Icon = "🧀",
                        Title = $"Варка #{shortBatchId}",
                        Details = $"Статус: {batch.BatchStatus} | Запуск: {batch.StartedAt:dd.MM.yyyy HH:mm}"
                    };

                    foreach (var aging in batch.AgingLots ?? [])
                    {
                        batchNode.Children.Add(new TraceTreeNode
                        {
                            Icon = "⏳",
                            Title = $"Камера: {aging.ChamberName}",
                            Details = $"Лот: {aging.AgingBatchNumber} ({aging.AgingStatus})"
                        });
                    }

                    rootNode.Children.Add(batchNode);
                }

                TreeNodes.Add(rootNode);
                SetStatus(UiConstants.Messages.DATA_LOADED_SUCCESS, Brushes.Green);
            }
            else
            {
                SetStatus("Связи трассировки для данной партии не найдены!", Brushes.OrangeRed);
            }
        }
        catch (Exception ex)
        {
            SetStatus($"{UiConstants.Messages.DATA_LOAD_ERROR}: {ex.Message}", Brushes.Red);
        }
    }

    private async Task RunBackwardTraceabilityAsync(Guid productStockLotId)
    {
        try
        {
            var endpoint = $"{Endpoints.TRACEABILITY_BACKWARD}/{productStockLotId}";
            var traceData = await ApiService.Instance.GetAsync<BackwardTraceabilityDto>(endpoint);

            if (traceData != null)
            {
                var customerInfo = string.IsNullOrEmpty(traceData.CustomerName) || traceData.CustomerName == FormattingConstants.CONST_DEFAULT_CUSTOMER_NAME
                    ? "На складе"
                    : traceData.CustomerName;

                var priceInfo = traceData.UnitPrice > 0 ? $" | Цена/С/С: ${traceData.UnitPrice:N2}" : string.Empty;
                var agingLossInfo = traceData.AgingLossPercentage > 0 ? $" | Усушка: {traceData.AgingLossPercentage:N2}%" : string.Empty;

                var rootNode = new TraceTreeNode
                {
                    Icon = FormattingConstants.ICON_FINISHED_PRODUCT,
                    Title = traceData.ProductName,
                    Details = $"Партия: {traceData.ProductBatchNumber} | Остаток: {traceData.CurrentStockQuantity:N2} кг | Покупатель: {customerInfo}{priceInfo}"
                };

                TraceTreeNode parentForBatch = rootNode;

                bool hasRealAging = traceData.AgingDaysTotal > 0 ||
                    (!string.IsNullOrWhiteSpace(traceData.StorageChamberName)
                     && traceData.StorageChamberName != FormattingConstants.CONST_DEFAULT_CHAMBER_NAME
                     && traceData.StorageChamberName != "Камера");

                if (hasRealAging)
                {
                    var agingNode = new TraceTreeNode
                    {
                        Icon = "⏳",
                        Title = $"Выдержка: {traceData.StorageChamberName}",
                        Details = $"Дней в камере: {traceData.AgingDaysTotal}{agingLossInfo}"
                    };

                    rootNode.Children.Add(agingNode);
                    parentForBatch = agingNode;
                }

                if (traceData.OriginBatch != null)
                {
                    var batchIdStr = traceData.OriginBatch.ProductionBatchId.ToString();
                    var shortBatchId = batchIdStr.Length >= 8 ? batchIdStr[..8].ToUpperInvariant() : batchIdStr;

                    var yieldInfo = traceData.OriginBatch.OutputYieldPercentage > 0
                        ? $" | Выход: {traceData.OriginBatch.ActualOutputQuantity:N2} кг ({traceData.OriginBatch.OutputYieldPercentage:N1}% от плана)"
                        : string.Empty;

                    var batchNode = new TraceTreeNode
                    {
                        Icon = FormattingConstants.ICON_PRODUCTION_BATCH,
                        Title = $"Варка #{shortBatchId}",
                        Details = $"Статус: {traceData.OriginBatch.BatchStatus} | Запуск: {traceData.OriginBatch.StartedAt:dd.MM.yyyy HH:mm}{yieldInfo}"
                    };

                    foreach (var ing in traceData.OriginBatch.ConsumedIngredients ?? [])
                    {
                        var supplier = !string.IsNullOrEmpty(ing.SupplierName) && ing.SupplierName != "—"
                            ? $" | Поставщик: {ing.SupplierName}"
                            : string.Empty;

                        var uom = !string.IsNullOrEmpty(ing.UnitOfMeasure) ? $" {ing.UnitOfMeasure}" : string.Empty;

                        batchNode.Children.Add(new TraceTreeNode
                        {
                            Icon = FormattingConstants.ICON_RAW_MATERIAL,
                            Title = ing.RawMaterialName,
                            Details = $"Партия: {ing.BatchNumber} | Расход: {ing.QuantityUsed:N2}{uom}{supplier}"
                        });
                    }

                    parentForBatch.Children.Add(batchNode);
                }

                TreeNodes.Add(rootNode);
                SetStatus(UiConstants.Messages.DATA_LOADED_SUCCESS, Brushes.Green);
            }
            else
            {
                SetStatus("Связи трассировки для данной партии не найдены!", Brushes.OrangeRed);
            }
        }
        catch (Exception ex)
        {
            SetStatus($"{UiConstants.Messages.DATA_LOAD_ERROR}: {ex.Message}", Brushes.Red);
        }
    }

    private void SetStatus(string msg, Brush color)
    {
        StatusTextBlock.Foreground = color;
        StatusTextBlock.Text = LocalizationService.Get(msg);
    }
}