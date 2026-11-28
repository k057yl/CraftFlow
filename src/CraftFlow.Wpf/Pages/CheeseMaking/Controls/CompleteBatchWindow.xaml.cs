using CraftFlow.Wpf.Models;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using static CraftFlow.Wpf.Services.LocalizationService;

namespace CraftFlow.Wpf.Pages.CheeseMaking.Controls;

public partial class CompleteBatchWindow : Window
{
    public ObservableCollection<CheeseHeadItemModel> CheeseHeads { get; } = [];

    public string CustomBatchNumber => BatchNumberTextBox.Text.Trim();
    public decimal TotalWeight => CheeseHeads.Where(h => h.Weight > 0).Sum(h => h.Weight);
    public int UnitsCount => CheeseHeads.Count(h => h.Weight > 0);
    public List<decimal> HeadWeights => CheeseHeads.Where(h => h.Weight > 0).Select(h => h.Weight).ToList();
    public decimal? OverheadPercentage { get; private set; }
    public DateTime? ExpirationDate => ExpirationDatePicker.SelectedDate;

    private readonly decimal _rawMaterialCost;
    private bool _isRebuilding;

    public CompleteBatchWindow(
        string batchName,
        string recipeName,
        decimal plannedOutput,
        decimal rawMaterialCost,
        decimal? existingOverheadPercentage = null)
    {
        InitializeComponent();

        _rawMaterialCost = rawMaterialCost;

        var kgUnit = Get("UI_UNIT_KG");
        RecipeTitleTextBlock.Text = string.Format(Get("UI_COMPLETE_WIN_RECIPE_FMT"), recipeName);
        PlannedInfoTextBlock.Text = string.Format(Get("UI_COMPLETE_WIN_PLANNED_FMT"), plannedOutput, kgUnit);
        BatchNumberTextBox.Text = batchName;

        if (existingOverheadPercentage.HasValue)
        {
            UseOverheadCheckBox.IsChecked = true;
            OverheadPercentageTextBox.Text = existingOverheadPercentage.Value.ToString("0.##", CultureInfo.InvariantCulture);
        }

        HeadsDataGrid.ItemsSource = CheeseHeads;

        TotalWeightInputTextBox.Text = plannedOutput > 0
            ? plannedOutput.ToString("0.###", CultureInfo.InvariantCulture)
            : string.Empty;

        RebuildHeads(1, plannedOutput);
        RecalculateSummary();
    }

    private static bool TryParseDecimal(string text, out decimal result)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            result = 0m;
            return false;
        }

        var normalized = text.Trim().Replace('.', ',');
        if (decimal.TryParse(normalized, out result)) return true;

        normalized = text.Trim().Replace(',', '.');
        return decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
    }

    private void RebuildHeads(int count, decimal totalWeight)
    {
        if (CheeseHeads == null) return;
        _isRebuilding = true;
        try
        {
            CheeseHeads.Clear();
            if (count <= 0) return;

            decimal avg = Math.Round(totalWeight / count, 3);
            decimal distributed = 0m;
            var prefix = string.IsNullOrWhiteSpace(BatchNumberTextBox.Text) ? "HEAD" : BatchNumberTextBox.Text.Trim();

            for (int i = 1; i <= count; i++)
            {
                decimal weight = (i == count)
                    ? Math.Max(0m, Math.Round(totalWeight - distributed, 3))
                    : avg;

                distributed += weight;

                CheeseHeads.Add(new CheeseHeadItemModel
                {
                    ItemCode = $"{prefix}-{i:D2}",
                    InitialWeight = weight,
                    Weight = weight
                });
            }
        }
        finally
        {
            _isRebuilding = false;
        }
    }

    private void UnitsCountTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isRebuilding || UnitsCountTextBox == null) return;

        if (int.TryParse(UnitsCountTextBox.Text.Trim(), out int count) && count > 0 && count <= 200)
        {
            TryParseDecimal(TotalWeightInputTextBox?.Text ?? string.Empty, out decimal totalWeight);
            if (totalWeight <= 0) totalWeight = TotalWeight;

            RebuildHeads(count, totalWeight);
            RecalculateSummary();
        }
    }

    private void DistributeEvenly_Click(object sender, RoutedEventArgs e)
    {
        if (!TryParseDecimal(TotalWeightInputTextBox.Text, out decimal totalWeight) || totalWeight <= 0)
        {
            MessageBox.Show(
                Get("UI_COMPLETE_WIN_ERR_INVALID_TOTAL_WEIGHT"),
                Get("UI_WEIGH_WIN_ERR_TITLE"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        int count = CheeseHeads.Count > 0 ? CheeseHeads.Count : 1;
        RebuildHeads(count, totalWeight);
        RecalculateSummary();
    }

    private void HeadsDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        Dispatcher.BeginInvoke(new Action(() =>
        {
            TotalWeightInputTextBox.Text = TotalWeight.ToString("0.###", CultureInfo.InvariantCulture);
            RecalculateSummary();
        }), System.Windows.Threading.DispatcherPriority.Background);
    }

    private void CostInputs_Changed(object sender, RoutedEventArgs e)
    {
        RecalculateSummary();
    }

    private void RecalculateSummary()
    {
        if (TotalWeightTextBlock == null || RawCostTextBlock == null || UnitCostTextBlock == null) return;

        var kgUnit = Get("UI_UNIT_KG");
        decimal totalWeight = TotalWeight;
        TotalWeightTextBlock.Text = $"{totalWeight:N3} {kgUnit}";

        decimal overheadPct = 0m;
        if (UseOverheadCheckBox.IsChecked == true && TryParseDecimal(OverheadPercentageTextBox.Text, out decimal parsedPct) && parsedPct >= 0)
        {
            overheadPct = parsedPct;
        }

        decimal overheadAmount = _rawMaterialCost * (overheadPct / 100m);
        decimal totalBatchCost = _rawMaterialCost + overheadAmount;
        decimal unitCost = totalWeight > 0 ? totalBatchCost / totalWeight : 0m;

        RawCostTextBlock.Text = $"$ {_rawMaterialCost:F2}";
        OverheadCostTextBlock.Text = $"+$ {overheadAmount:F2}";
        TotalCostTextBlock.Text = $"$ {totalBatchCost:F2}";
        UnitCostTextBlock.Text = $"$ {unitCost:F2}";
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
        if (UnitsCount == 0 || TotalWeight <= 0)
        {
            MessageBox.Show(
                Get("UI_COMPLETE_WIN_ERR_NO_WEIGHT"),
                Get("UI_WEIGH_WIN_ERR_TITLE"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        if (UseOverheadCheckBox.IsChecked == true)
        {
            if (TryParseDecimal(OverheadPercentageTextBox.Text, out decimal pct) && pct >= 0)
            {
                OverheadPercentage = pct;
            }
            else
            {
                MessageBox.Show(
                    Get("UI_COMPLETE_WIN_ERR_INVALID_OVERHEAD"),
                    Get("UI_WEIGH_WIN_ERR_TITLE"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }
        }
        else
        {
            OverheadPercentage = null;
        }

        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}