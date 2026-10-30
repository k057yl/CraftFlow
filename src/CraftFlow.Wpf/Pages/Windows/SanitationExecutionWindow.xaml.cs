using CraftFlow.Api.Modules.Identity;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Constants.Endpoints;
using CraftFlow.SharedKernel.Dtos.Common;
using CraftFlow.SharedKernel.Enums.Sanitation;
using CraftFlow.Wpf.Services;
using System.Text.Json;
using System.Windows;
using static CraftFlow.Wpf.Services.LocalizationService;

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

        var titleFmt = Get("UI_SANITATION_TITLE_FMT");
        TargetTitleTextBlock.Text = string.Format(titleFmt, targetName);

        ChemicalsTextBox.Text = string.IsNullOrWhiteSpace(defaultChemicals)
            ? Get("UI_SANITATION_DEFAULT_CHEMICAL")
            : defaultChemicals;

        SwitchMode(_activeTaskId.HasValue);

        Loaded += async (s, e) => await LoadUsersAsync();
    }

    private async Task LoadUsersAsync()
    {
        try
        {
            var users = await ApiService.Instance.GetAsync<List<LookupDto>>($"{IdentityConstants.USERS}/lookup");
            if (users != null && users.Count > 0)
            {
                PerformerComboBox.ItemsSource = users;

                var currentUserId = ApiService.Instance.CurrentUser?.Id;
                if (currentUserId.HasValue && users.Any(u => u.Id == currentUserId.Value))
                {
                    PerformerComboBox.SelectedValue = currentUserId.Value;
                }
                else
                {
                    PerformerComboBox.SelectedIndex = 0;
                }
                return;
            }
        }
        catch
        {
        }

        var currentUser = ApiService.Instance.CurrentUser;
        if (currentUser != null)
        {
            PerformerComboBox.ItemsSource = new List<LookupDto>
            {
                new(currentUser.Id, currentUser.FullName)
            };
            PerformerComboBox.SelectedIndex = 0;
        }
    }

    private void SwitchMode(bool isUnderSanitation)
    {
        StartPanel.Visibility = isUnderSanitation ? Visibility.Collapsed : Visibility.Visible;
        CompletePanel.Visibility = isUnderSanitation ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void Start_Click(object sender, RoutedEventArgs e)
    {
        var selectedPerformerId = PerformerComboBox.SelectedValue is Guid userId
            ? userId
            : ApiService.Instance.CurrentUser?.Id;

        var payload = new
        {
            TargetType = _targetType,
            TargetId = _targetId,
            PerformerUserId = selectedPerformerId,
            Notes = StartNotesTextBox.Text.Trim()
        };

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync($"{SanitationConstants.TASKS}/start-new", payload);

        if (isSuccess && !string.IsNullOrWhiteSpace(contentOrError) && Guid.TryParse(contentOrError.Trim('"'), out var taskId))
        {
            _activeTaskId = taskId;
            AlertWindow.ShowSuccess(this, Get("UI_SANITATION_MSG_STARTED"));
            DialogResult = true;
        }
        else
        {
            string rawError = contentOrError ?? string.Empty;
            string errorCode = ExtractErrorCode(rawError);

            if (errorCode.Equals("INV_LOCATION_HAS_VOLUME", StringComparison.OrdinalIgnoreCase) ||
                errorCode.Equals(ErrorCodes.Inventory.LOCATION_HAS_VOLUME, StringComparison.OrdinalIgnoreCase) ||
                rawError.Contains("LOCATION_HAS_VOLUME", StringComparison.OrdinalIgnoreCase))
            {
                var volumeMsg = Get("UI_SANITATION_ERR_LOCATION_HAS_VOLUME");
                AlertWindow.ShowError(this, !string.IsNullOrWhiteSpace(volumeMsg)
                    ? volumeMsg
                    : "Нельзя начать мойку: в ёмкости находится продукция.");
            }
            else
            {
                var localized = Get($"ERR_{errorCode}");
                if (!string.IsNullOrEmpty(localized) && !localized.StartsWith("ERR_"))
                {
                    AlertWindow.ShowError(this, localized);
                }
                else
                {
                    AlertWindow.ShowError(this, !string.IsNullOrWhiteSpace(errorCode) && errorCode != "UNKNOWN_ERROR"
                        ? errorCode
                        : "Нельзя начать мойку: проверьте состояние ёмкости.");
                }
            }
        }
    }

    private static string ExtractErrorCode(string rawResponse)
    {
        if (string.IsNullOrWhiteSpace(rawResponse))
            return "UNKNOWN_ERROR";

        try
        {
            using var doc = JsonDocument.Parse(rawResponse);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("code", out var codeProp) || root.TryGetProperty("Code", out codeProp))
                {
                    var val = codeProp.GetString();
                    if (!string.IsNullOrWhiteSpace(val)) return val;
                }

                if (root.TryGetProperty("error", out var errProp) || root.TryGetProperty("Error", out errProp))
                {
                    if (errProp.ValueKind == JsonValueKind.Object)
                    {
                        if (errProp.TryGetProperty("code", out var innerCode) || errProp.TryGetProperty("Code", out innerCode))
                        {
                            var val = innerCode.GetString();
                            if (!string.IsNullOrWhiteSpace(val)) return val;
                        }
                    }
                    else if (errProp.ValueKind == JsonValueKind.String)
                    {
                        var val = errProp.GetString();
                        if (!string.IsNullOrWhiteSpace(val)) return val;
                    }
                }
            }
        }
        catch
        {
        }

        return rawResponse;
    }

    private async void CompleteAndVerify_Click(object sender, RoutedEventArgs e)
    {
        if (!_activeTaskId.HasValue) return;

        var chemicals = ChemicalsTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(chemicals))
        {
            AlertWindow.ShowError(this, Get("UI_SANITATION_ERR_CHEMICALS_REQUIRED"));
            return;
        }

        var userId = ApiService.Instance.CurrentUser?.Id ?? Guid.Empty;

        var (completeSuccess, completeErr) = await ApiService.Instance.PostAndReadAsync($"{SanitationConstants.TASKS}/complete", new
        {
            TaskId = _activeTaskId.Value,
            ChemicalsUsed = chemicals,
            Notes = Get("UI_SANITATION_NOTE_COMPLETED_BY_OPERATOR")
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
            AlertWindow.ShowSuccess(this, Get("UI_SANITATION_MSG_ACCEPTED"));
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
            VerificationNotes = Get("UI_SANITATION_NOTE_REWASH_REJECTED")
        });

        AlertWindow.ShowWarning(this, Get("UI_SANITATION_MSG_REJECTED"));
        DialogResult = true;
    }

    private void Close_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}