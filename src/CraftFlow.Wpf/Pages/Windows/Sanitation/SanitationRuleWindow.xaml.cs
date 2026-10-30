using CraftFlow.SharedKernel.Enums.Sanitation;
using CraftFlow.Wpf.Services;
using System.Windows;
using System.Windows.Controls;
using static CraftFlow.Wpf.Services.LocalizationService;

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
            SanitationTargetType.WAREHOUSE => Get("UI_SANITATION_TARGET_WAREHOUSE"),
            SanitationTargetType.AGING_CHAMBER => Get("UI_SANITATION_TARGET_CHAMBER"),
            _ => Get("UI_SANITATION_TARGET_LOCATION")
        };

        EquipmentTitleTextBlock.Text = $"{typeTitle}: {objectName}";

        if (ScheduledDatePicker != null)
        {
            ScheduledDatePicker.SelectedDate = DateTime.Today.AddDays(7);
        }

        TriggerTypeComboBox.SelectedIndex = 3;
    }

    private void TriggerType_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (BatchPanel == null || DaysPanel == null || DatePanel == null)
            return;

        int selectedIndex = TriggerTypeComboBox.SelectedIndex;

        BatchPanel.Visibility = (selectedIndex == 0 || selectedIndex == 3) ? Visibility.Visible : Visibility.Collapsed;
        DaysPanel.Visibility = (selectedIndex == 1 || selectedIndex == 3) ? Visibility.Visible : Visibility.Collapsed;
        DatePanel.Visibility = (selectedIndex == 2) ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void SaveRule_Click(object sender, RoutedEventArgs e)
    {
        var triggerType = TriggerTypeComboBox.SelectedIndex switch
        {
            1 => SanitationTriggerType.INTERVAL_DAYS,
            2 => SanitationTriggerType.SCHEDULED_DATE,
            3 => SanitationTriggerType.BOTH,
            4 => SanitationTriggerType.UPON_EMPTY,
            _ => SanitationTriggerType.BATCH_COUNT
        };

        int.TryParse(BatchIntervalTextBox.Text, out var batchInterval);
        int.TryParse(DaysIntervalTextBox.Text, out var daysInterval);
        DateTime? scheduledDate = ScheduledDatePicker?.SelectedDate;

        var payload = new
        {
            TargetType = _targetType,
            TargetId = _targetId,
            TriggerType = triggerType,
            RequiredChemicals = ChemicalsTextBox.Text.Trim(),
            IntervalDays = (triggerType == SanitationTriggerType.INTERVAL_DAYS || triggerType == SanitationTriggerType.BOTH) && daysInterval > 0 ? daysInterval : (int?)null,
            MaxBatchCount = (triggerType == SanitationTriggerType.BATCH_COUNT || triggerType == SanitationTriggerType.BOTH) && batchInterval > 0 ? batchInterval : (int?)null,
            NextScheduledAt = triggerType == SanitationTriggerType.SCHEDULED_DATE ? scheduledDate : (DateTime?)null
        };

        var result = await ApiService.Instance.PostAndReadAsync("api/sanitation/rules", payload);

        if (result.IsSuccess)
        {
            AlertWindow.ShowSuccess(this, Get("UI_SANITATION_RULE_MSG_SUCCESS"));
            DialogResult = true;
        }
        else
        {
            AlertWindow.ShowError(this, $"{Get("UI_SANITATION_RULE_MSG_ERROR")}: {result.ContentOrError}");
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}   