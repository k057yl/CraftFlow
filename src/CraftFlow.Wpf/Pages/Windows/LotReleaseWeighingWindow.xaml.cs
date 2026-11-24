using CraftFlow.SharedKernel.Dtos.Aging;
using CraftFlow.SharedKernel.Enums.Inventory;
using CraftFlow.Wpf.Models;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace CraftFlow.Wpf.Pages.Production.Windows;

public partial class LotReleaseWeighingWindow : Window
{
    public ObservableCollection<CheeseHeadItemModel> CheeseHeads { get; } = [];

    public decimal TotalWeight => CheeseHeads.Where(h => !h.IsDiscarded && h.Weight > 0).Sum(h => h.Weight);
    public int ValidUnitsCount => CheeseHeads.Count(h => !h.IsDiscarded && h.Weight > 0);
    public List<decimal> ValidHeadWeights => CheeseHeads.Where(h => !h.IsDiscarded && h.Weight > 0).Select(h => h.Weight).ToList();
    public List<Guid> DiscardedItemIds => CheeseHeads.Where(h => h.IsDiscarded && h.ItemId != Guid.Empty).Select(h => h.ItemId).ToList();
    public decimal UnitPrice { get; private set; }

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

        LotNameTextBlock.Text = $"Партия: {lotName}";
        ChamberInfoTextBlock.Text = $"Камера: {chamberName}";

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

        TotalUnitsTextBlock.Text = $"{validCount} шт";
        InitialTotalWeightTextBlock.Text = $"{initialWeight:N3} кг";
        TotalWeightTextBlock.Text = $"{totalWeight:N3} кг";

        if (initialWeight > 0 && totalWeight > 0)
        {
            var shrinkagePct = ((initialWeight - totalWeight) / initialWeight) * 100m;
            TotalShrinkageTextBlock.Text = $"Усушка: {shrinkagePct:F1}% (-{Math.Max(0, initialWeight - totalWeight):N3} кг)";
        }
        else
        {
            TotalShrinkageTextBlock.Text = "Усушка: —";
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

    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        if (ValidUnitsCount == 0 || TotalWeight <= 0)
        {
            MessageBox.Show("Внесите вес хотя бы одной годной головки сыра!", "Ошибка взвешивания", MessageBoxButton.OK, MessageBoxImage.Warning);
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