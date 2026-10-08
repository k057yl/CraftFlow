using CraftFlow.Api.Modules.Aging;
using CraftFlow.Api.Modules.Production;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Dtos.Aging;
using CraftFlow.SharedKernel.Dtos.Common;
using CraftFlow.SharedKernel.Dtos.Production;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Services;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace CraftFlow.Wpf.Pages.Production;

public partial class ProductionPage : Page
{
    public ObservableCollection<LookupItem> Warehouses => BrewhouseView.Warehouses;
    public ObservableCollection<LookupItem> Recipes => BrewhouseView.Recipes;
    public ObservableCollection<LookupItem> ActiveBatches => BrewhouseView.ActiveBatches;
    public ObservableCollection<ActiveBatchWpfModel> ActiveBatchesSummary => BrewhouseView.ActiveBatchesSummary;

    public ObservableCollection<BatchReadyForAgingDto> CompletedBatches { get; } = [];
    public ObservableCollection<LookupItem> AgingChambers { get; } = [];
    public ObservableCollection<LookupItem> ActiveAgingLots { get; } = [];
    public ObservableCollection<AgingLotSummaryDto> AgingLotsSummary { get; } = [];

    private readonly DispatcherTimer _uiTimer = new();

    public ProductionPage()
    {
        InitializeComponent();

        BrewhouseView.StatusTextBlock = StatusTextBlock;
        AgingView.StatusTextBlock = StatusTextBlock;
        DiscardView.StatusTextBlock = StatusTextBlock;

        BrewhouseView.ParentPage = this;
        AgingView.ParentPage = this;
        DiscardView.ParentPage = this;

        AgingView.Warehouses = Warehouses;
        AgingView.CompletedBatches = CompletedBatches;
        AgingView.AgingChambers = AgingChambers;
        AgingView.ActiveAgingLots = ActiveAgingLots;
        AgingView.AgingLotsSummary = AgingLotsSummary;

        DiscardView.ActiveBatches = ActiveBatches;
        DiscardView.AgingLotsSummary = AgingLotsSummary;

        _uiTimer.Interval = TimeSpan.FromSeconds(1);
        _uiTimer.Tick += UiTimer_Tick;
        _uiTimer.Start();

        Loaded += async (s, e) => await LoadDataAsync();
    }

    private void UiTimer_Tick(object? sender, EventArgs e)
    {
        foreach (var batch in ActiveBatchesSummary)
        {
            batch.CurrentElapsed = batch.CurrentElapsed.Add(TimeSpan.FromSeconds(1));
        }
    }

    public async Task LoadDataExternalAsync()
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            await BrewhouseView.LoadDataAsync();

            var readyBatches = await ApiService.Instance.GetAsync<List<BatchReadyForAgingDto>>(ProductionConstants.BATCHES_READY_AGING);
            CompletedBatches.Clear();
            readyBatches?.ForEach(CompletedBatches.Add);

            var chambers = await ApiService.Instance.GetAsync<List<LookupDto>>(AgingConstants.AGING_CHAMBERS);
            AgingChambers.Clear();
            chambers?.ForEach(c => AgingChambers.Add(new LookupItem(c.Id, c.Name)));

            ActiveAgingLots.Clear();
            AgingLotsSummary.Clear();

            try
            {
                var activeSummary = await ApiService.Instance.GetAsync<List<AgingLotSummaryDto>>(AgingConstants.AGING_LOTS_ACTIVE);
                if (activeSummary != null && activeSummary.Count > 0)
                {
                    foreach (var lot in activeSummary)
                    {
                        AgingLotsSummary.Add(lot);
                        ActiveAgingLots.Add(new LookupItem(lot.LotId, lot.BatchNumber));
                    }
                }
            }
            catch { }

            SetStatus(UiConstants.Messages.DATA_LOADED_SUCCESS, Brushes.Green);
        }
        catch (Exception ex)
        {
            SetStatusFormatted(UiConstants.Messages.DATA_LOAD_ERROR, Brushes.Red, ex.Message);
        }
    }

    private void SetStatus(string resourceKey, Brush color)
    {
        StatusTextBlock.Foreground = color;
        StatusTextBlock.Text = LocalizationService.Get(resourceKey);
    }

    private void SetStatusFormatted(string resourceKey, Brush color, params object[] args)
    {
        StatusTextBlock.Foreground = color;
        var format = LocalizationService.Get(resourceKey);
        StatusTextBlock.Text = string.Format(format, args);
    }
}