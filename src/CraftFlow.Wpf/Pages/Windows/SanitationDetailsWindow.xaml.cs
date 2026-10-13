using CraftFlow.SharedKernel.Enums.Sanitation;
using CraftFlow.Wpf.Services;
using System.Windows;

namespace CraftFlow.Wpf.Windows;

public partial class SanitationDetailsWindow : Window
{
    private readonly Guid _targetId;
    private readonly SanitationTargetType _targetType;
    private readonly string _targetName;

    public SanitationDetailsWindow(Guid targetId, string targetName, SanitationTargetType targetType)
    {
        InitializeComponent();
        _targetId = targetId;
        _targetName = targetName;
        _targetType = targetType;

        Loaded += async (s, e) => await LoadDetailsAsync();
    }

    private async Task LoadDetailsAsync()
    {
        try
        {
            var data = await ApiService.Instance.GetAsync<SanitationDetailsResponseDto>($"api/sanitation/details/{_targetType}/{_targetId}");
            if (data != null)
            {
                TargetTitleTextBlock.Text = $"{GetTypeName(_targetType)}: {data.TargetName}";
                StatusTextBlock.Text = $"Статус: {data.StatusText}";

                LastWashDateTextBlock.Text = $"Дата: {(data.LastWashedAt.HasValue ? data.LastWashedAt.Value.ToString("dd.MM.yyyy HH:mm") : "—")}";
                LastWashedByTextBlock.Text = $"Кто убирал: {data.LastWashedByUserName}";

                NextWashDateTextBlock.Text = $"Срок: {(data.NextWashDueDate.HasValue ? data.NextWashDueDate.Value.ToString("dd.MM.yyyy") : "—")}";
                ChemicalsTextBlock.Text = $"Химия: {data.RequiredChemicals ?? "—"}";

                LogsDataGrid.ItemsSource = data.Logs;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static string GetTypeName(SanitationTargetType type) => type switch
    {
        SanitationTargetType.Warehouse => "Склад",
        SanitationTargetType.AgingChamber => "Камера",
        _ => "Ёмкость"
    };

    private async void EditRules_Click(object sender, RoutedEventArgs e)
    {
        var ruleWindow = new SanitationRuleWindow(_targetId, _targetName, _targetType)
        {
            Owner = Window.GetWindow(this)
        };

        if (ruleWindow.ShowDialog() == true)
        {
            await LoadDetailsAsync();
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}

public record SanitationLogResponseDto(
    Guid Id,
    string PerformedByUserName,
    string VerifiedByUserName,
    DateTime CompletedAt,
    DateTime VerifiedAt,
    int DurationMinutes,
    string ChemicalsUsed,
    string? Notes,
    bool IsPassed
);

public record SanitationDetailsResponseDto(
    Guid TargetId,
    SanitationTargetType TargetType,
    string TargetName,
    SanitationTriggerType? TriggerType,
    int? IntervalDays,
    int? MaxBatchCount,
    string? RequiredChemicals,
    DateTime? LastWashedAt,
    string LastWashedByUserName,
    DateTime? NextWashDueDate,
    string StatusText,
    List<SanitationLogResponseDto> Logs
);