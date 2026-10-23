using CraftFlow.Api.Modules.Identity;
using CraftFlow.Api.Modules.Subscriptions.Domain;
using CraftFlow.SharedKernel.Dtos.Identity;
using CraftFlow.SharedKernel.Dtos.Subscription;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.Wpf.Services;
using CraftFlow.Wpf.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CraftFlow.Wpf.Pages;

public partial class ProfilePage : Page
{
    private const string ENDPOINT_SUBSCRIPTION_KEYS = "api/subscriptions/keys";
    private const string ENDPOINT_SUBSCRIPTION_KEYS_MY = "api/subscriptions/keys/my";
    private const string ENDPOINT_ACTIVATE_PLAN = "api/subscriptions/confirm";

    private const string MSG_KEY_NAME_REQUIRED = "Пожалуйста, введите название для API ключа!";
    private const string MSG_KEY_GENERATED_SUCCESS = "Ключ успешно сгенерирован и сохранен.";
    private const string MSG_PLAN_ACTIVATED_SUCCESS = "Тариф успешно изменён! Все лимиты пересчитаны.";

    private const string MSG_CAPTION_INFO = "Информация";
    private const string MSG_CAPTION_WARNING = "Внимание";
    private const string MSG_CAPTION_ERROR = "Ошибка";

    public ProfilePage()
    {
        InitializeComponent();
        InitRoleComboBox();
        InitPlanComboBox();
        LoadUserData();
    }

    private void InitRoleComboBox()
    {
        NewUserRoleComboBox.ItemsSource = new[]
        {
            new { Role = TenantRole.ACCOUNTANT, Display = "Бухгалтер" },
            new { Role = TenantRole.TECHNOLOGIST, Display = "Главный технолог" },
            new { Role = TenantRole.OPERATOR, Display = "Оператор / Сыровар" },
            new { Role = TenantRole.STOREKEEPER, Display = "Кладовщик" },
            new { Role = TenantRole.SALES_MANAGER, Display = "Менеджер по продажам" },
            new { Role = TenantRole.AUDITOR, Display = "Аудитор" }
        };
        NewUserRoleComboBox.DisplayMemberPath = "Display";
        NewUserRoleComboBox.SelectedValuePath = "Role";
        NewUserRoleComboBox.SelectedIndex = 0;
    }

    private void InitPlanComboBox()
    {
        PlanComboBox.ItemsSource = new[]
        {
            new { Code = "FREE", Display = "Free Trial (8 партий, 2 склада)" },
            new { Code = "STANDARD", Display = "Standard (32 партии, 5 складов)" },
            new { Code = "PRO", Display = "Professional (Безлимит)" }
        };
        PlanComboBox.DisplayMemberPath = "Display";
        PlanComboBox.SelectedValuePath = "Code";
        PlanComboBox.SelectedIndex = 0;
    }

    private async void LoadUserData()
    {
        var user = ApiService.Instance.CurrentUser;
        if (user == null) return;

        NameTextBlock.Text = user.FullName;
        EmailTextBlock.Text = user.Email;
        RoleTextBlock.Text = GetRoleDisplayName(user.Role);

        if (string.IsNullOrWhiteSpace(user.Email))
        {
            var profile = await ApiService.Instance.GetAsync<UserProfileDto>("api/identity/profile");
            if (profile != null && !string.IsNullOrWhiteSpace(profile.Email))
            {
                user.Email = profile.Email;
                user.Role = profile.Role;
                EmailTextBlock.Text = profile.Email;
                RoleTextBlock.Text = GetRoleDisplayName(profile.Role);
            }
        }

        bool isOwner = user.Role == TenantRole.OWNER;
        bool isSuperAdmin = user.Role == TenantRole.SUPER_ADMIN;

        TeamManagementBorder.Visibility = (isOwner || isSuperAdmin) ? Visibility.Visible : Visibility.Collapsed;
        SubscriptionManagementBorder.Visibility = isSuperAdmin ? Visibility.Visible : Visibility.Collapsed;
        DangerZoneBorder.Visibility = (isOwner || isSuperAdmin) ? Visibility.Visible : Visibility.Collapsed;

        if (isOwner || isSuperAdmin)
        {
            await LoadTeamUsersAsync();
            if (isSuperAdmin)
            {
                await LoadAccessKeysAsync();
            }
        }
    }

    private async void ActivateSelectedPlan_Click(object sender, RoutedEventArgs e)
    {
        var selectedCode = PlanComboBox.SelectedValue?.ToString();
        if (string.IsNullOrEmpty(selectedCode)) return;

        var payload = new
        {
            PlanCode = selectedCode,
            Days = 30
        };

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(ENDPOINT_ACTIVATE_PLAN, payload);

        if (isSuccess)
        {
            MessageBox.Show(MSG_PLAN_ACTIVATED_SUCCESS, MSG_CAPTION_INFO, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show(contentOrError, MSG_CAPTION_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LoadAccessKeysAsync()
    {
        var keys = await ApiService.Instance.GetAsync<List<AccessKeyDto>>(ENDPOINT_SUBSCRIPTION_KEYS_MY);

        if (keys != null)
        {
            var viewModels = keys.Select(k => new
            {
                k.Id,
                k.Name,
                KeyPreview = k.MaskedKey,
                StatusDisplay = k.Status.ToString(),
                CanRevoke = k.Status != KeyStatus.Revoked
            }).ToList();

            AccessKeysDataGrid.ItemsSource = viewModels;
        }
    }

    private async void GenerateAccessKey_Click(object sender, RoutedEventArgs e)
    {
        var keyName = NewAccessKeyNameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(keyName))
        {
            MessageBox.Show(MSG_KEY_NAME_REQUIRED, MSG_CAPTION_WARNING, MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var payload = new { KeyName = keyName };
        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(ENDPOINT_SUBSCRIPTION_KEYS, payload);

        if (isSuccess)
        {
            NewAccessKeyNameTextBox.Clear();
            await LoadAccessKeysAsync();
            MessageBox.Show(MSG_KEY_GENERATED_SUCCESS, MSG_CAPTION_INFO, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show(contentOrError, MSG_CAPTION_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void RevokeAccessKey_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Guid keyId)
        {
            var endpointUrl = $"{ENDPOINT_SUBSCRIPTION_KEYS}/{keyId}";
            var (isSuccess, contentOrError) = await ApiService.Instance.DeleteAndReadAsync(endpointUrl);

            if (isSuccess)
            {
                await LoadAccessKeysAsync();
            }
            else
            {
                MessageBox.Show(contentOrError, MSG_CAPTION_ERROR, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async Task LoadTeamUsersAsync()
    {
        var users = await ApiService.Instance.GetTenantUsersAsync();
        var currentUserId = ApiService.Instance.CurrentUser?.Email;

        var viewModels = users
            .Where(u => u.Role != TenantRole.SUPER_ADMIN)
            .Select(u => new
            {
                u.Id,
                u.FullName,
                u.Email,
                RoleDisplay = GetRoleDisplayName(u.Role),
                StatusDisplay = u.IsActive ? "Активен" : "Заблокирован",
                ActionButtonText = u.IsActive ? "Деактивировать" : "Активировать",
                CanToggle = u.Role != TenantRole.OWNER && u.Email != currentUserId
            }).ToList();

        TeamUsersDataGrid.ItemsSource = viewModels;
    }

    private async void ToggleUserStatus_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Guid userId)
        {
            var (isSuccess, contentOrError) = await ApiService.Instance.ToggleUserStatusAsync(userId);
            if (isSuccess)
            {
                await LoadTeamUsersAsync();
            }
            else
            {
                MessageBox.Show(contentOrError, "Ошибка смены статуса", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private async void CreateTeamUser_Click(object sender, RoutedEventArgs e)
    {
        var fullName = NewUserFullNameTextBox.Text.Trim();
        var email = NewUserEmailTextBox.Text.Trim();
        var password = NewUserPasswordBox.Password;

        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            MessageBox.Show("Заполните все поля для создания нового сотрудника!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var selectedRole = (TenantRole)(NewUserRoleComboBox.SelectedValue ?? TenantRole.TECHNOLOGIST);
        var requestPayload = new CreateTenantUserRequest(email, password, fullName, selectedRole);

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(IdentityConstants.USERS, requestPayload);

        if (isSuccess)
        {
            MessageBox.Show($"Сотрудник {fullName} успешно создан!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            NewUserFullNameTextBox.Clear();
            NewUserEmailTextBox.Clear();
            NewUserPasswordBox.Clear();
            await LoadTeamUsersAsync();
        }
        else
        {
            MessageBox.Show(contentOrError, "Ошибка при создании", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void DeleteAccount_Click(object sender, RoutedEventArgs e)
    {
        var user = ApiService.Instance.CurrentUser;
        if (user == null) return;

        var dialog = new ConfirmDeleteAccountWindow(user.Email)
        {
            Owner = Window.GetWindow(this)
        };

        if (dialog.ShowDialog() != true) return;

        string targetEndpoint = dialog.ActionType == AccountDeleteAction.Deactivate
            ? $"{IdentityConstants.DELETE_ACCOUNT}/deactivate"
            : $"{IdentityConstants.DELETE_ACCOUNT}/confirm";

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(targetEndpoint, new
        {
            ConfirmationEmail = dialog.EnteredEmail
        });

        if (isSuccess)
        {
            string message = dialog.ActionType == AccountDeleteAction.Deactivate
                ? "Ваш аккаунт деактивирован. Вы можете восстановить доступ в любое время."
                : "Ваш аккаунт и все данные сыроварни были полностью стерты.";

            MessageBox.Show(message, "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);

            ApiService.Instance.ClearAuthToken();

            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.NavigateToAuth();
            }
        }
        else
        {
            StatusTextBlock.Foreground = Brushes.Red;
            StatusTextBlock.Text = contentOrError.Trim('"');
        }
    }

    private static string GetRoleDisplayName(TenantRole role) => role switch
    {
        TenantRole.SUPER_ADMIN => "Супер Администратор",
        TenantRole.OWNER => "Владелец сыроварни",
        TenantRole.ACCOUNTANT => "Бухгалтер",
        TenantRole.TECHNOLOGIST => "Главный технолог",
        TenantRole.OPERATOR => "Оператор / Сыровар",
        TenantRole.STOREKEEPER => "Кладовщик",
        TenantRole.SALES_MANAGER => "Менеджер по продажам",
        TenantRole.AUDITOR => "Аудитор",
        _ => "Сотрудник"
    };
}