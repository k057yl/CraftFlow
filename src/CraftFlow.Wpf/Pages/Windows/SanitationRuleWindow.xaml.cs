using CraftFlow.SharedKernel.Enums.Sanitation;
using CraftFlow.Wpf.Services;
using System.Windows;
using System.Windows.Controls;

namespace CraftFlow.Wpf.Windows;

public partial class SanitationRuleWindow : Window
{
    private readonly Guid _targetId;
    private readonly SanitationTargetType _targetType;

    public SanitationRuleWindow(
        Guid targetId,
        string objectName,
        SanitationTargetType targetType = SanitationTargetType.STORAGE_LOCATION)
    {
        InitializeComponent();
        _targetId = targetId;
        _targetType = targetType;

        var typeTitle = targetType switch
        {
            SanitationTargetType.WAREHOUSE => "Склад",
            SanitationTargetType.AGING_CHAMBER => "Камера",
            _ => "Ёмкость"
        };

        EquipmentTitleTextBlock.Text = $"{typeTitle}: {objectName}";
    }

    private void TriggerType_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (BatchIntervalTextBox == null || DaysIntervalTextBox == null) return;

        int selectedIndex = TriggerTypeComboBox.SelectedIndex;

        bool enableBatch = selectedIndex == 0 || selectedIndex == 3;
        bool enableDays = selectedIndex == 1 || selectedIndex == 3;

        BatchIntervalTextBox.IsEnabled = enableBatch;
        DaysIntervalTextBox.IsEnabled = enableDays;
    }

    private async void SaveRule_Click(object sender, RoutedEventArgs e)
    {
        var triggerType = TriggerTypeComboBox.SelectedIndex switch
        {
            1 => SanitationTriggerType.INTERVAL_DAYS,
            2 => SanitationTriggerType.SCHEDULED_DATE,
            3 => SanitationTriggerType.BOTH,
            _ => SanitationTriggerType.BATCH_COUNT
        };

        int.TryParse(BatchIntervalTextBox.Text, out var batchInterval);
        int.TryParse(DaysIntervalTextBox.Text, out var daysInterval);

        var result = await ApiService.Instance.PostAndReadAsync("api/sanitation/rules", new
        {
            TargetType = _targetType,
            TargetId = _targetId,
            TriggerType = triggerType,
            RequiredChemicals = ChemicalsTextBox.Text.Trim(),
            IntervalDays = daysInterval > 0 ? daysInterval : (int?)null,
            MaxBatchCount = batchInterval > 0 ? batchInterval : (int?)null
        });

        if (result.IsSuccess)
        {
            MessageBox.Show("Регламент мойки успешно обновлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
        }
        else
        {
            MessageBox.Show($"Ошибка сохранения: {result.ContentOrError}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}