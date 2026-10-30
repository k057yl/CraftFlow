using CraftFlow.SharedKernel.Dtos.Sanitation;
using CraftFlow.SharedKernel.Enums.Sanitation;
using CraftFlow.Wpf.Services;
using System.Windows;

namespace CraftFlow.Wpf.Windows;

public partial class SanitationDetailsWindow : Window
{
    private readonly Guid _targetId;
    private readonly SanitationTargetType _targetType;
    private readonly string _targetName;
    private string _defaultChemicals = string.Empty;
    private Guid? _activeTaskId;

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
            var data = await ApiService.Instance.GetAsync<SanitationDetailsDto>($"api/sanitation/details/{_targetType}/{_targetId}");
            if (data != null)
            {
                TargetTitleTextBlock.Text = $"{GetTypeName(_targetType)}: {data.TargetName}";
                StatusTextBlock.Text = $"Статус: {data.StatusText}";

                _activeTaskId = data.ActiveTaskId;

                if (data.IsUnderSanitation)
                {
                    StartSanitationButton.Visibility = Visibility.Collapsed;
                    CurrentSanitationButton.Visibility = Visibility.Visible;
                }
                else
                {
                    StartSanitationButton.Visibility = Visibility.Visible;
                    CurrentSanitationButton.Visibility = Visibility.Collapsed;
                }

                LastWashDateTextBlock.Text = $"Дата: {(data.LastWashedAt.HasValue ? data.LastWashedAt.Value.ToString("dd.MM.yyyy HH:mm") : "—")}";
                LastWashedByTextBlock.Text = $"Кто убирал: {data.LastWashedByUserName}";

                NextWashDateTextBlock.Text = $"Срок: {(data.NextWashDueDate.HasValue ? data.NextWashDueDate.Value.ToString("dd.MM.yyyy") : "—")}";

                _defaultChemicals = data.RequiredChemicals ?? "Щелочной раствор CIP-30";
                ChemicalsTextBlock.Text = $"Химия: {_defaultChemicals}";

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
        SanitationTargetType.WAREHOUSE => "Склад",
        SanitationTargetType.AGING_CHAMBER => "Камера",
        _ => "Ёмкость"
    };

    private async void PerformSanitation_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SanitationExecutionWindow(_targetId, _targetName, _targetType, _activeTaskId, _defaultChemicals)
        {
            Owner = Window.GetWindow(this)
        };

        if (dialog.ShowDialog() == true)
        {
            await LoadDetailsAsync();
        }
    }

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