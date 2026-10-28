using CraftFlow.Api.Modules.Aging;
using CraftFlow.Api.Modules.Production;
using CraftFlow.SharedKernel.Dtos.Aging;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static CraftFlow.Wpf.Services.LocalizationService;

namespace CraftFlow.Wpf.Pages.Production.Controls;

public partial class DiscardActsControl : UserControl
{
    private ObservableCollection<LookupItem> _activeBatches = [];
    public ObservableCollection<LookupItem> ActiveBatches
    {
        get => _activeBatches;
        set
        {
            _activeBatches = value;
            DiscardBatchComboBox.ItemsSource = _activeBatches;
        }
    }

    private ObservableCollection<AgingLotSummaryDto> _agingLotsSummary = [];
    public ObservableCollection<AgingLotSummaryDto> AgingLotsSummary
    {
        get => _agingLotsSummary;
        set
        {
            _agingLotsSummary = value;
            DiscardAgingLotsDataGrid.ItemsSource = _agingLotsSummary;
        }
    }

    public TextBlock? StatusTextBlock { get; set; }
    public ProductionPage? ParentPage { get; set; }

    public DiscardActsControl()
    {
        InitializeComponent();

        DiscardBatchComboBox.ItemsSource = ActiveBatches;
        DiscardAgingLotsDataGrid.ItemsSource = AgingLotsSummary;
    }

    private async void DiscardBatch_Click(object sender, RoutedEventArgs e)
    {
        if (DiscardBatchComboBox.SelectedValue is not Guid batchId ||
            string.IsNullOrWhiteSpace(DiscardReasonTextBox.Text))
        {
            SetStatus("UI_ERR_INVALID_INPUT_FIELDS", Brushes.Red);
            return;
        }

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(ProductionConstants.BATCHES_DISCARD, new
        {
            BatchId = batchId,
            Reason = DiscardReasonTextBox.Text.Trim()
        });

        if (isSuccess)
        {
            SetStatus("UI_DISCARD_MSG_BATCH_SUCCESS", Brushes.OrangeRed);

            DiscardBatchComboBox.SelectedValue = null;
            DiscardReasonTextBox.Clear();

            if (ParentPage != null)
            {
                await ParentPage.LoadDataExternalAsync();
            }
        }
        else
        {
            SetStatusRaw(contentOrError, Brushes.Red);
        }
    }

    private async void DiscardAgingLotRow_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is Guid lotId)
        {
            var lot = AgingLotsSummary.FirstOrDefault(l => l.LotId == lotId);
            if (lot == null) return;

            try
            {
                var items = await ApiService.Instance.GetAsync<List<GetAgingLotItemDto>>($"{AgingConstants.AGING_LOTS_ACTIVE}/{lotId}/items");
                var dialog = new WriteOffDialog(lot.BatchNumber, lot.ChamberName, lot.InitialQuantity, items)
                {
                    Owner = Window.GetWindow(this)
                };

                if (dialog.ShowDialog() == true)
                {
                    bool isSuccess;
                    string contentOrError;

                    if (dialog.SelectedItemIds.Count > 0)
                    {
                        var res = await ApiService.Instance.PostAndReadAsync(AgingConstants.AGING_LOTS_DISCARD_ITEMS, new
                        {
                            AgingLotId = lotId,
                            ItemIds = dialog.SelectedItemIds,
                            Reason = dialog.Reason
                        });

                        isSuccess = res.IsSuccess;
                        contentOrError = res.ContentOrError;
                    }
                    else
                    {
                        var res = await ApiService.Instance.PostAndReadAsync($"{AgingConstants.AGING_LOTS_ACTIVE}/discard", new
                        {
                            AgingLotId = lotId,
                            Quantity = dialog.QuantityToWriteOff,
                            UnitsToRemove = 0,
                            Reason = dialog.Reason
                        });

                        isSuccess = res.IsSuccess;
                        contentOrError = res.ContentOrError;
                    }

                    if (isSuccess)
                    {
                        SetStatus("UI_DISCARD_MSG_LOT_SUCCESS", Brushes.OrangeRed);

                        if (ParentPage != null)
                        {
                            await ParentPage.LoadDataExternalAsync();
                        }
                    }
                    else
                    {
                        SetStatusRaw(contentOrError, Brushes.Red);
                    }
                }
            }
            catch (Exception ex)
            {
                SetStatusFormatted("UI_ERR_DATA_LOAD_FMT", Brushes.Red, ex.Message);
            }
        }
    }

    private void SetStatus(string resourceKey, Brush color)
    {
        if (StatusTextBlock == null) return;
        StatusTextBlock.Foreground = color;
        StatusTextBlock.Text = Get(resourceKey);
    }

    private void SetStatusFormatted(string resourceKey, Brush color, params object[] args)
    {
        if (StatusTextBlock == null) return;
        StatusTextBlock.Foreground = color;
        var format = Get(resourceKey);
        StatusTextBlock.Text = string.Format(format, args);
    }

    private void SetStatusRaw(string rawText, Brush color)
    {
        if (StatusTextBlock == null) return;
        StatusTextBlock.Foreground = color;
        StatusTextBlock.Text = rawText;
    }
}