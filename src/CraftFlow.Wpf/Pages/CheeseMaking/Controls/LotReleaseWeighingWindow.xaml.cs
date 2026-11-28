using CraftFlow.SharedKernel.Dtos.Aging;
using CraftFlow.SharedKernel.Enums.Inventory;
using CraftFlow.Wpf.Models;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using static CraftFlow.Wpf.Services.LocalizationService;

namespace CraftFlow.Wpf.Pages.CheeseMaking.Controls;

public partial class LotReleaseWeighingWindow : Window
{
    public ObservableCollection<CheeseHeadItemModel> CheeseHeads { get; } = [];

    public decimal TotalWeight => CheeseHeads.Where(h => !h.IsDiscarded && h.Weight > 0).Sum(h => h.Weight);
    public int ValidUnitsCount => CheeseHeads.Count(h => !h.IsDiscarded && h.Weight > 0);
    public List<decimal> ValidHeadWeights => CheeseHeads.Where(h => !h.IsDiscarded && h.Weight > 0).Select(h => h.Weight).ToList();
    public List<Guid> DiscardedItemIds => CheeseHeads.Where(h => h.IsDiscarded && h.ItemId != Guid.Empty).Select(h => h.ItemId).ToList();
    public decimal UnitPrice { get; private set; }
    public DateTime? ExpirationDate => ExpirationDatePicker.SelectedDate;

    private readonly decimal _totalBatchCost;

    public LotReleaseWeighingWindow(
        string lotName,
        string chamberName,
        decimal totalBatchCost,
        int initialUnitsCount,
        IReadOnlyList<GetAgingLotItemDto>? items = null)
    {
        InitializeComponent();

        _totalBatchCost = totalBatchCost;

        LotNameTextBlock.Text = string.Format(Get("UI_WEIGH_WIN_LOT_FMT"), lotName);
        ChamberInfoTextBlock.Text = string.Format(Get("UI_WEIGH_WIN_CHAMBER_FMT"), chamberName);

        ItemsDataGrid.ItemsSource = CheeseHeads;

        var activeItems = items?.Where(i => i.State == (int)AgingItemState.IN_CHAMBER).ToList();

        if (activeItems != null && activeItems.Count > 0)
        {
            foreach (var item in activeItems)
            {
                CheeseHeads.Add(new CheeseHeadItemModel
                {
                    ItemId = item.Id,
                    ItemCode = item.ItemNumber,
                    InitialWeight = item.InitialWeight,
                    Weight = item.CurrentWeight > 0 ? item.CurrentWeight : item.InitialWeight
                });
            }
        }
        else
        {
            for (int i = 1; i <= Math.Max(1, initialUnitsCount); i++)
            {
                CheeseHeads.Add(new CheeseHeadItemModel
                {
                    ItemCode = $"{lotName}-#{i:D2}",
                    InitialWeight = 0m,
                    Weight = 0m
                });
            }
        }

        RecalculateSummary();
    }

    private void ItemsDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        Dispatcher.BeginInvoke(new Action(RecalculateSummary), System.Windows.Threading.DispatcherPriority.Background);
    }

    private void CheckBox_Click(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(new Action(RecalculateSummary), System.Windows.Threading.DispatcherPriority.Background);
    }

    private void RecalculateSummary()
    {
        var validHeads = CheeseHeads.Where(h => !h.IsDiscarded && h.Weight > 0).ToList();
        var totalWeight = validHeads.Sum(h => h.Weight);
        var initialWeight = validHeads.Sum(h => h.InitialWeight);
        var validCount = validHeads.Count;
        var kgUnit = Get("UI_UNIT_KG");
        var pcsUnit = Get("UI_UNIT_PCS");

        TotalUnitsTextBlock.Text = $"{validCount} {pcsUnit}";
        InitialTotalWeightTextBlock.Text = $"{initialWeight:N3} {kgUnit}";
        TotalWeightTextBlock.Text = $"{totalWeight:N3} {kgUnit}";

        if (initialWeight > 0 && totalWeight > 0)
        {
            var shrinkagePct = ((initialWeight - totalWeight) / initialWeight) * 100m;
            var lossKg = Math.Max(0, initialWeight - totalWeight);
            TotalShrinkageTextBlock.Text = string.Format(Get("UI_WEIGH_WIN_SHRINKAGE_FMT"), shrinkagePct, lossKg, kgUnit);
        }
        else
        {
            TotalShrinkageTextBlock.Text = Get("UI_WEIGH_WIN_SHRINKAGE_EMPTY");
        }

        if (totalWeight > 0)
        {
            var calculatedCost = _totalBatchCost > 0 ? _totalBatchCost / totalWeight : 0m;
            CalculatedCostTextBlock.Text = $"${calculatedCost:F2}";

            if (string.IsNullOrWhiteSpace(UnitPriceTextBox.Text))
            {
                UnitPriceTextBox.Text = calculatedCost.ToString("F2", CultureInfo.InvariantCulture);
            }
        }
        else
        {
            CalculatedCostTextBlock.Text = "$ 0.00";
        }
    }

    private void AddDays_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && int.TryParse(btn.Tag?.ToString(), out int days))
        {
            ExpirationDatePicker.SelectedDate = DateTime.Today.AddDays(days);
        }
    }

    private void ClearDate_Click(object sender, RoutedEventArgs e)
    {
        ExpirationDatePicker.SelectedDate = null;
    }

    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        if (ValidUnitsCount == 0 || TotalWeight <= 0)
        {
            MessageBox.Show(
                Get("UI_WEIGH_WIN_ERR_NO_VALID_HEADS"),
                Get("UI_WEIGH_WIN_ERR_TITLE"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        decimal.TryParse(UnitPriceTextBox.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var price);
        UnitPrice = price;

        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}