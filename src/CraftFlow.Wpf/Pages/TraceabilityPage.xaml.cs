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
    Guid SalesOrderId,
    string CustomerName,
    Guid ProductStockLotId,
    string ProductBatchNumber,
    string ProductName,
    TraceabilityProductionBatchDto OriginBatch
);

public record TraceabilityProductionBatchDto(
    Guid ProductionBatchId,
    string BatchStatus,
    DateTime StartedAt,
    DateTime? CompletedAt,
    List<TraceabilityAgingLotDto> AgingLots,
    List<TraceabilityIngredientDto> UsedIngredients
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
    decimal QuantityUsed
);

// --- UI Модели и Страница ---

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

        Loaded += async (s, e) => await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var stockLots = await ApiService.Instance.GetAsync<List<LookupDto>>(Endpoints.STOCK_LOTS);
            StockLots.Clear();
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

            if (traceData != null)
            {
                var rootNode = new TraceTreeNode
                {
                    Icon = FormattingConstants.ICON_RAW_MATERIAL,
                    Title = traceData.RawMaterialName,
                    Details = string.Format(FormattingConstants.TRACE_RAW_DETAILS_FORMAT, traceData.RawMaterialBatchNumber)
                };

                foreach (var batch in traceData.Batches ?? [])
                {
                    var batchIdStr = batch.ProductionBatchId.ToString();
                    var shortBatchId = batchIdStr.Length >= 4 ? batchIdStr.Substring(0, 4).ToUpperInvariant() : batchIdStr;

                    var batchNode = new TraceTreeNode
                    {
                        Icon = FormattingConstants.ICON_PRODUCTION_BATCH,
                        Title = string.Format(FormattingConstants.TRACE_BATCH_TITLE_FORMAT, shortBatchId),
                        Details = string.Format(FormattingConstants.TRACE_BATCH_DETAILS_FORMAT, batch.BatchStatus, batch.StartedAt.ToString("dd.MM.yyyy"))
                    };

                    foreach (var aging in batch.AgingLots ?? [])
                    {
                        batchNode.Children.Add(new TraceTreeNode
                        {
                            Icon = FormattingConstants.ICON_AGING_LOT,
                            Title = string.Format(FormattingConstants.TRACE_AGING_TITLE_FORMAT, aging.ChamberName),
                            Details = string.Format(FormattingConstants.TRACE_AGING_DETAILS_FORMAT, aging.AgingBatchNumber, aging.AgingStatus)
                        });
                    }

                    rootNode.Children.Add(batchNode);
                }

                TreeNodes.Add(rootNode);
                SetStatus(UiConstants.Messages.DATA_LOADED_SUCCESS, Brushes.Green);
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
                var rootNode = new TraceTreeNode
                {
                    Icon = "🧀",
                    Title = traceData.ProductName,
                    Details = $"Партия ГП #{traceData.ProductBatchNumber} | Покупатель: {traceData.CustomerName}"
                };

                if (traceData.OriginBatch != null)
                {
                    var batchIdStr = traceData.OriginBatch.ProductionBatchId.ToString();
                    var shortBatchId = batchIdStr.Length >= 4 ? batchIdStr.Substring(0, 4).ToUpperInvariant() : batchIdStr;

                    var batchNode = new TraceTreeNode
                    {
                        Icon = FormattingConstants.ICON_PRODUCTION_BATCH,
                        Title = $"Варка #{shortBatchId}",
                        Details = $"Статус: {traceData.OriginBatch.BatchStatus} (Дата: {traceData.OriginBatch.StartedAt:dd.MM.yyyy})"
                    };

                    foreach (var ing in traceData.OriginBatch.UsedIngredients ?? [])
                    {
                        batchNode.Children.Add(new TraceTreeNode
                        {
                            Icon = FormattingConstants.ICON_RAW_MATERIAL,
                            Title = ing.RawMaterialName,
                            Details = $"Партия сырья #{ing.BatchNumber} (Списано: {ing.QuantityUsed:F2})"
                        });
                    }

                    rootNode.Children.Add(batchNode);
                }

                TreeNodes.Add(rootNode);
                SetStatus(UiConstants.Messages.DATA_LOADED_SUCCESS, Brushes.Green);
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