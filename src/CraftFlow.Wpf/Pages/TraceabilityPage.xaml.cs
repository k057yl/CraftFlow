using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Services;

namespace CraftFlow.Wpf.Pages;

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

        try
        {
            var endpoint = $"{Endpoints.TRACEABILITY_FORWARD}/{stockLotId}";
            var traceData = await ApiService.Instance.GetAsync<ForwardTraceabilityDto>(endpoint);

            TreeNodes.Clear();
            if (traceData != null)
            {
                var rootNode = new TraceTreeNode
                {
                    Icon = FormattingConstants.ICON_RAW_MATERIAL,
                    Title = traceData.RawMaterialName,
                    Details = string.Format(FormattingConstants.TRACE_RAW_DETAILS_FORMAT, traceData.RawMaterialBatchNumber)
                };

                foreach (var batch in traceData.Batches)
                {
                    var shortBatchId = batch.ProductionBatchId.ToString()[..4].ToUpperInvariant();

                    var batchNode = new TraceTreeNode
                    {
                        Icon = FormattingConstants.ICON_PRODUCTION_BATCH,
                        Title = string.Format(FormattingConstants.TRACE_BATCH_TITLE_FORMAT, shortBatchId),
                        Details = string.Format(FormattingConstants.TRACE_BATCH_DETAILS_FORMAT, batch.BatchStatus, batch.StartedAt)
                    };

                    foreach (var aging in batch.AgingLots)
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

    private void SetStatus(string msg, Brush color)
    {
        StatusTextBlock.Foreground = color;
        StatusTextBlock.Text = LocalizationService.Get(msg);
    }
}