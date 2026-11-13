using CraftFlow.SharedKernel.Constants.Endpoints;
using CraftFlow.SharedKernel.Dtos.Fermentation;
using CraftFlow.SharedKernel.Enums;
using CraftFlow.SharedKernel.Enums.Fermrntation;
using CraftFlow.Wpf.Services;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static CraftFlow.Wpf.Services.LocalizationService;

namespace CraftFlow.Wpf.Pages.Fermentation;

public partial class FermentationPage : Page
{
    public sealed class FermentationBatchUiModel
    {
        public Guid Id { get; init; }
        public Guid ProductionBatchId { get; init; }
        public string BatchNumber { get; init; } = string.Empty;
        public string RecipeName { get; init; } = string.Empty;
        public decimal SubstrateWeightKg { get; init; }
        public decimal SaltPercentage { get; init; }
        public decimal CalculatedSaltGrams { get; init; }
        public decimal? CurrentPh { get; init; }
        public string VesselInfo { get; init; } = string.Empty;
        public FermentationStage Stage { get; init; }
        public int DaysElapsed { get; init; }
        public List<FermentationReadingDto> Readings { get; init; } = [];

        public string StageDisplay => Stage switch
        {
            FermentationStage.ACTIVE_WARM => Get("UI_FERM_STAGE_WARM"),
            FermentationStage.COLD_RETARDATION => Get("UI_FERM_STAGE_COLD"),
            _ => Get("UI_FERM_STAGE_DONE")
        };

        public string SaltDisplay => $"{SaltPercentage:0.##}% ({CalculatedSaltGrams:0.#} г)";
        public string PhDisplay => CurrentPh.HasValue ? CurrentPh.Value.ToString("0.00", CultureInfo.InvariantCulture) : "—";
        public bool IsSafeAcidity => CurrentPh.HasValue && CurrentPh.Value < 4.6m;
    }

    public sealed class FermentationReadingUiModel
    {
        public string Summary { get; init; } = string.Empty;
    }

    public ObservableCollection<FermentationBatchUiModel> FermentationBatches { get; } = [];
    public ObservableCollection<FermentationReadingUiModel> SelectedBatchReadings { get; } = [];

    public FermentationPage()
    {
        InitializeComponent();

        FermentationBrewhouseView.TargetModule = AppModule.FERMENTATION;
        FermentationBrewhouseView.StatusTextBlock = StatusTextBlock;

        FermentationDiscardView.StatusTextBlock = StatusTextBlock;
        FermentationDiscardView.ActiveBatches = FermentationBrewhouseView.ActiveBatches;

        ActiveProdBatchComboBox.ItemsSource = FermentationBrewhouseView.ActiveBatches;
        FermentationBatchesDataGrid.ItemsSource = FermentationBatches;
        ReadingsHistoryListBox.ItemsSource = SelectedBatchReadings;

        UpdateSaltCalculator();

        Loaded += async (s, e) => await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            await FermentationBrewhouseView.LoadDataAsync();

            if (ActiveProdBatchComboBox.SelectedIndex < 0 && FermentationBrewhouseView.ActiveBatches.Count > 0)
            {
                ActiveProdBatchComboBox.SelectedIndex = 0;
            }

            var selectedId = (FermentationBatchesDataGrid.SelectedItem as FermentationBatchUiModel)?.Id;
            var batches = await ApiService.Instance.GetAsync<List<FermentationBatchDto>>($"{FermentationConstants.BATCHES}?includeCompleted=false");

            FermentationBatches.Clear();
            if (batches != null)
            {
                foreach (var b in batches)
                {
                    FermentationBatches.Add(new FermentationBatchUiModel
                    {
                        Id = b.Id,
                        ProductionBatchId = b.ProductionBatchId,
                        BatchNumber = b.BatchNumber,
                        RecipeName = b.RecipeName,
                        SubstrateWeightKg = b.SubstrateWeightKg,
                        SaltPercentage = b.SaltPercentage,
                        CalculatedSaltGrams = b.CalculatedSaltGrams,
                        CurrentPh = b.CurrentPh,
                        VesselInfo = b.VesselInfo,
                        Stage = b.Stage,
                        DaysElapsed = b.DaysElapsed,
                        Readings = b.Readings
                    });
                }
            }

            if (selectedId.HasValue)
            {
                FermentationBatchesDataGrid.SelectedItem = FermentationBatches.FirstOrDefault(x => x.Id == selectedId.Value);
            }
            else if (FermentationBatches.Count > 0)
            {
                FermentationBatchesDataGrid.SelectedIndex = 0;
            }
            else
            {
                SelectedBatchReadings.Clear();
            }

            SetStatus("UI_MSG_DATA_LOADED_SUCCESS", Brushes.Green);
        }
        catch (Exception ex)
        {
            SetStatusFormatted("UI_ERR_DATA_LOAD_FMT", Brushes.Red, ex.Message);
        }
    }

    private static bool TryParseDecimal(string text, out decimal result)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            result = 0;
            return false;
        }

        var normalized = text.Trim().Replace('.', ',');
        if (decimal.TryParse(normalized, out result)) return true;

        normalized = text.Trim().Replace(',', '.');
        return decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
    }

    private void SaltInputs_Changed(object sender, TextChangedEventArgs e)
    {
        UpdateSaltCalculator();
    }

    private void UpdateSaltCalculator()
    {
        if (CalculatedSaltTextBlock == null || SubstrateWeightTextBox == null || SaltPercentageTextBox == null) return;

        if (TryParseDecimal(SubstrateWeightTextBox.Text, out var weightKg) && weightKg > 0 &&
            TryParseDecimal(SaltPercentageTextBox.Text, out var saltPct) && saltPct >= 0)
        {
            var grams = Math.Round(weightKg * 1000m * (saltPct / 100m), 1);
            CalculatedSaltTextBlock.Text = string.Format(Get("UI_FERM_LABEL_SALT_GRAMS_FMT"), grams);
        }
        else
        {
            CalculatedSaltTextBlock.Text = string.Format(Get("UI_FERM_LABEL_SALT_GRAMS_FMT"), 0m);
        }
    }

    private async void CreateFermentationPassport_Click(object sender, RoutedEventArgs e)
    {
        if (ActiveProdBatchComboBox.SelectedValue is not Guid prodBatchId || prodBatchId == Guid.Empty ||
            !TryParseDecimal(SubstrateWeightTextBox.Text, out var substrateKg) || substrateKg <= 0 ||
            !TryParseDecimal(SaltPercentageTextBox.Text, out var saltPct) || saltPct < 0 ||
            !TryParseDecimal(TargetTempTextBox.Text, out var targetTemp))
        {
            SetStatus("UI_ERR_INVALID_INPUT_FIELDS", Brushes.Red);
            return;
        }

        decimal? initialPh = null;
        if (!string.IsNullOrWhiteSpace(InitialPhTextBox.Text))
        {
            if (TryParseDecimal(InitialPhTextBox.Text, out var parsedPh) && parsedPh > 0 && parsedPh <= 14)
            {
                initialPh = parsedPh;
            }
            else
            {
                SetStatus("UI_ERR_INVALID_INPUT_FIELDS", Brushes.Red);
                return;
            }
        }

        var (isSuccess, error) = await ApiService.Instance.PostAndReadAsync(FermentationConstants.BATCHES, new
        {
            ProductionBatchId = prodBatchId,
            SubstrateWeightKg = substrateKg,
            SaltPercentage = saltPct,
            TargetTemperature = targetTemp,
            VesselInfo = VesselInfoTextBox.Text?.Trim() ?? string.Empty,
            InitialPh = initialPh
        });

        if (isSuccess)
        {
            InitialPhTextBox.Clear();
            VesselInfoTextBox.Clear();
            await LoadDataAsync();
            SetStatus("UI_FERM_MSG_PASSPORT_CREATED", Brushes.Green);
        }
        else
        {
            SetStatusRaw(error, Brushes.Red);
        }
    }

    private void FermentationBatchesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SelectedBatchReadings.Clear();
        if (FermentationBatchesDataGrid.SelectedItem is not FermentationBatchUiModel selected) return;

        foreach (var r in selected.Readings)
        {
            var dateStr = r.RecordedAt.ToLocalTime().ToString("dd.MM HH:mm");
            var phStr = r.Ph.HasValue ? $"pH {r.Ph.Value:0.00}" : string.Empty;
            var tempStr = r.Temperature.HasValue ? $"{r.Temperature.Value:0.#}°C" : string.Empty;
            var actions = string.Join(", ", new[]
            {
                r.DegassedOrBurped ? "CO2" : null,
                r.BrineToppedUp ? "+Brine" : null
            }.Where(x => x != null));

            var parts = new[] { dateStr, phStr, tempStr, actions, r.SensoryNotes }
                .Where(p => !string.IsNullOrWhiteSpace(p));

            SelectedBatchReadings.Add(new FermentationReadingUiModel
            {
                Summary = string.Join(" | ", parts)
            });
        }
    }

    private async void AddReading_Click(object sender, RoutedEventArgs e)
    {
        if (FermentationBatchesDataGrid.SelectedItem is not FermentationBatchUiModel selected)
        {
            SetStatus("UI_ERR_INVALID_INPUT_FIELDS", Brushes.Red);
            return;
        }

        decimal? ph = null;
        if (!string.IsNullOrWhiteSpace(ReadingPhTextBox.Text))
        {
            if (!TryParseDecimal(ReadingPhTextBox.Text, out var parsedPh) || parsedPh <= 0 || parsedPh > 14)
            {
                SetStatus("UI_ERR_INVALID_INPUT_FIELDS", Brushes.Red);
                return;
            }
            ph = parsedPh;
        }

        decimal? temp = null;
        if (!string.IsNullOrWhiteSpace(ReadingTempTextBox.Text))
        {
            if (!TryParseDecimal(ReadingTempTextBox.Text, out var parsedTemp))
            {
                SetStatus("UI_ERR_INVALID_INPUT_FIELDS", Brushes.Red);
                return;
            }
            temp = parsedTemp;
        }

        var url = $"{FermentationConstants.BATCHES}/{selected.Id}/{FermentationConstants.READINGS}";
        var (isSuccess, error) = await ApiService.Instance.PostAndReadAsync(url, new
        {
            Ph = ph,
            Temperature = temp,
            DegassedOrBurped = DegassedCheckBox.IsChecked == true,
            BrineToppedUp = BrineToppedUpCheckBox.IsChecked == true,
            SensoryNotes = string.IsNullOrWhiteSpace(SensoryNotesTextBox.Text) ? null : SensoryNotesTextBox.Text.Trim()
        });

        if (isSuccess)
        {
            ReadingPhTextBox.Clear();
            ReadingTempTextBox.Clear();
            DegassedCheckBox.IsChecked = false;
            BrineToppedUpCheckBox.IsChecked = false;
            SensoryNotesTextBox.Clear();

            await LoadDataAsync();
            SetStatus("UI_FERM_MSG_READING_ADDED", Brushes.Green);
        }
        else
        {
            SetStatusRaw(error, Brushes.Red);
        }
    }

    private async void MoveToCold_Click(object sender, RoutedEventArgs e)
    {
        if (FermentationBatchesDataGrid.SelectedItem is not FermentationBatchUiModel selected) return;

        var url = $"{FermentationConstants.BATCHES}/{selected.Id}/{FermentationConstants.MOVE_TO_COLD}";
        var (isSuccess, error) = await ApiService.Instance.PostAndReadAsync(url, new { });

        if (isSuccess)
        {
            await LoadDataAsync();
            SetStatus("UI_FERM_MSG_MOVED_COLD", Brushes.Green);
        }
        else
        {
            SetStatusRaw(error, Brushes.Red);
        }
    }

    private async void CompleteFermentation_Click(object sender, RoutedEventArgs e)
    {
        if (FermentationBatchesDataGrid.SelectedItem is not FermentationBatchUiModel selected) return;

        var url = $"{FermentationConstants.BATCHES}/{selected.Id}/{FermentationConstants.COMPLETE}";
        var (isSuccess, error) = await ApiService.Instance.PostAndReadAsync(url, new { });

        if (isSuccess)
        {
            await LoadDataAsync();
            SetStatus("UI_FERM_MSG_COMPLETED", Brushes.Green);
        }
        else
        {
            SetStatusRaw(error, Brushes.Red);
        }
    }

    private void SetStatus(string resourceKey, Brush color)
    {
        StatusTextBlock.Foreground = color;
        StatusTextBlock.Text = Get(resourceKey);
    }

    private void SetStatusFormatted(string resourceKey, Brush color, params object[] args)
    {
        StatusTextBlock.Foreground = color;
        StatusTextBlock.Text = string.Format(Get(resourceKey), args);
    }

    private void SetStatusRaw(string rawText, Brush color)
    {
        StatusTextBlock.Foreground = color;
        StatusTextBlock.Text = rawText;
    }
}