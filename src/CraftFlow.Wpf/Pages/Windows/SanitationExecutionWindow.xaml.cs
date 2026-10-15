using CraftFlow.SharedKernel.Constants.Endpoints;
using CraftFlow.SharedKernel.Enums.Sanitation;
using CraftFlow.Wpf.Services;
using System.Windows;

namespace CraftFlow.Wpf.Windows;

public partial class SanitationExecutionWindow : Window
{
    private readonly Guid _targetId;
    private readonly SanitationTargetType _targetType;
    private Guid? _activeTaskId;

    public SanitationExecutionWindow(
        Guid targetId,
        string targetName,
        SanitationTargetType targetType,
        Guid? activeTaskId = null,
        string defaultChemicals = "")
    {
        InitializeComponent();
        _targetId = targetId;
        _targetType = targetType;
        _activeTaskId = activeTaskId;

        TargetTitleTextBlock.Text = $"Мойка: {targetName}";
        ChemicalsTextBox.Text = string.IsNullOrWhiteSpace(defaultChemicals) ? "Щелочной раствор CIP-30" : defaultChemicals;
        PerformerTextBox.Text = ApiService.Instance.CurrentUser?.FullName ?? "Сотрудник";

        SwitchMode(_activeTaskId.HasValue);
    }

    private void SwitchMode(bool isUnderSanitation)
    {
        StartPanel.Visibility = isUnderSanitation ? Visibility.Collapsed : Visibility.Visible;
        CompletePanel.Visibility = isUnderSanitation ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void Start_Click(object sender, RoutedEventArgs e)
    {
        var payload = new
        {
            TargetType = _targetType,
            TargetId = _targetId
        };

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync($"{SanitationConstants.TASKS}/start-new", payload);

        if (isSuccess && Guid.TryParse(contentOrError.Trim('"'), out var taskId))
        {
            _activeTaskId = taskId;
            AlertWindow.ShowSuccess(this, "Мойка запущена! Объект переведён в статус обслуживания.");
            DialogResult = true;
        }
        else
        {
            AlertWindow.ShowError(this, contentOrError);
        }
    }

    private async void CompleteAndVerify_Click(object sender, RoutedEventArgs e)
    {
        if (!_activeTaskId.HasValue) return;

        var chemicals = ChemicalsTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(chemicals))
        {
            AlertWindow.ShowError(this, "Укажите использованные химические средства.");
            return;
        }

        var userId = ApiService.Instance.CurrentUser?.Id ?? Guid.Empty;

        var (completeSuccess, completeErr) = await ApiService.Instance.PostAndReadAsync($"{SanitationConstants.TASKS}/complete", new
        {
            TaskId = _activeTaskId.Value,
            ChemicalsUsed = chemicals,
            Notes = "Завершено оператором"
        });

        if (!completeSuccess)
        {
            AlertWindow.ShowError(this, completeErr);
            return;
        }

        var (verifySuccess, verifyErr) = await ApiService.Instance.PostAndReadAsync(SanitationConstants.TASKS_VERIFY, new
        {
            TaskId = _activeTaskId.Value,
            VerifierUserId = userId,
            IsPassed = PassedCheckBox.IsChecked ?? true,
            VerificationNotes = VerificationNotesTextBox.Text.Trim()
        });

        if (verifySuccess)
        {
            AlertWindow.ShowSuccess(this, "Мойка принята! Объект доступен к работе.");
            DialogResult = true;
        }
        else
        {
            AlertWindow.ShowError(this, verifyErr);
        }
    }

    private async void ReWash_Click(object sender, RoutedEventArgs e)
    {
        if (!_activeTaskId.HasValue) return;

        var userId = ApiService.Instance.CurrentUser?.Id ?? Guid.Empty;

        await ApiService.Instance.PostAndReadAsync(SanitationConstants.TASKS_VERIFY, new
        {
            TaskId = _activeTaskId.Value,
            VerifierUserId = userId,
            IsPassed = false,
            VerificationNotes = "Забраковано. Направлено на повторную мойку."
        });

        AlertWindow.ShowWarning(this, "Мойка отклонена. Объект отправлен на повторную обработку.");
        DialogResult = true;
    }

    private void Close_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}