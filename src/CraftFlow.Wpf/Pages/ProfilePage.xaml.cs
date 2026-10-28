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
using static CraftFlow.Wpf.Services.LocalizationService;

namespace CraftFlow.Wpf.Pages;

public partial class ProfilePage : Page
{
    private const string ENDPOINT_SUBSCRIPTION_KEYS = "api/subscriptions/keys";
    private const string ENDPOINT_SUBSCRIPTION_KEYS_MY = "api/subscriptions/keys/my";
    private const string ENDPOINT_ACTIVATE_PLAN = "api/subscriptions/confirm";

    public ProfilePage() : this(null)
    {
    }

    public ProfilePage(object? dummy = null)
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
            new { Role = TenantRole.ACCOUNTANT, Display = Get("UI_ROLE_ACCOUNTANT") },
            new { Role = TenantRole.TECHNOLOGIST, Display = Get("UI_ROLE_TECHNOLOGIST") },
            new { Role = TenantRole.OPERATOR, Display = Get("UI_ROLE_OPERATOR") },
            new { Role = TenantRole.STOREKEEPER, Display = Get("UI_ROLE_STOREKEEPER") },
            new { Role = TenantRole.SALES_MANAGER, Display = Get("UI_ROLE_SALES_MANAGER") },
            new { Role = TenantRole.AUDITOR, Display = Get("UI_ROLE_AUDITOR") }
        };
        NewUserRoleComboBox.DisplayMemberPath = "Display";
        NewUserRoleComboBox.SelectedValuePath = "Role";
        NewUserRoleComboBox.SelectedIndex = 0;
    }

    private void InitPlanComboBox()
    {
        PlanComboBox.ItemsSource = new[]
        {
            new { Code = "FREE", Display = Get("UI_PLAN_FREE_DISPLAY") },
            new { Code = "STANDARD", Display = Get("UI_PLAN_STANDARD_DISPLAY") },
            new { Code = "PRO", Display = Get("UI_PLAN_PRO_DISPLAY") }
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

        bool isOwner = PermissionService.Instance.IsOwner;
        bool isSuperAdmin = PermissionService.Instance.IsSuperAdmin;

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
            MessageBox.Show(Get("UI_MSG_PLAN_ACTIVATED_SUCCESS"), Get("UI_CAPTION_INFO"), MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show(contentOrError, Get("UI_CAPTION_ERROR"), MessageBoxButton.OK, MessageBoxImage.Error);
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
            MessageBox.Show(Get("UI_MSG_KEY_NAME_REQUIRED"), Get("UI_CAPTION_WARNING"), MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var payload = new { KeyName = keyName };
        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(ENDPOINT_SUBSCRIPTION_KEYS, payload);

        if (isSuccess)
        {
            NewAccessKeyNameTextBox.Clear();
            await LoadAccessKeysAsync();
            MessageBox.Show(Get("UI_MSG_KEY_GENERATED_SUCCESS"), Get("UI_CAPTION_INFO"), MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show(contentOrError, Get("UI_CAPTION_ERROR"), MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show(contentOrError, Get("UI_CAPTION_ERROR"), MessageBoxButton.OK, MessageBoxImage.Error);
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
                StatusDisplay = u.IsActive ? Get("UI_STATUS_ACTIVE") : Get("UI_STATUS_BLOCKED"),
                ActionButtonText = u.IsActive ? Get("UI_BTN_DEACTIVATE") : Get("UI_BTN_ACTIVATE"),
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
                MessageBox.Show(contentOrError, Get("UI_ERR_STATUS_CHANGE"), MessageBoxButton.OK, MessageBoxImage.Error);
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
            MessageBox.Show(Get("UI_ERR_FILL_ALL_NEW_EMPLOYEE_FIELDS"), Get("UI_CAPTION_WARNING"), MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var selectedRole = (TenantRole)(NewUserRoleComboBox.SelectedValue ?? TenantRole.TECHNOLOGIST);
        var requestPayload = new CreateTenantUserRequest(email, password, fullName, selectedRole);

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(IdentityConstants.USERS, requestPayload);

        if (isSuccess)
        {
            var msgFormat = Get("UI_MSG_EMPLOYEE_CREATED_FMT");
            MessageBox.Show(string.Format(msgFormat, fullName), Get("UI_CAPTION_SUCCESS"), MessageBoxButton.OK, MessageBoxImage.Information);
            NewUserFullNameTextBox.Clear();
            NewUserEmailTextBox.Clear();
            NewUserPasswordBox.Clear();
            await LoadTeamUsersAsync();
        }
        else
        {
            MessageBox.Show(contentOrError, Get("UI_ERR_CREATE_USER"), MessageBoxButton.OK, MessageBoxImage.Error);
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
                ? Get("UI_MSG_ACCOUNT_DEACTIVATED_INFO")
                : Get("UI_MSG_ACCOUNT_DELETED_INFO");

            MessageBox.Show(message, Get("UI_CAPTION_NOTIFICATION"), MessageBoxButton.OK, MessageBoxImage.Information);

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
        TenantRole.SUPER_ADMIN => Get("UI_ROLE_SUPER_ADMIN"),
        TenantRole.OWNER => Get("UI_ROLE_OWNER"),
        TenantRole.ACCOUNTANT => Get("UI_ROLE_ACCOUNTANT"),
        TenantRole.TECHNOLOGIST => Get("UI_ROLE_TECHNOLOGIST"),
        TenantRole.OPERATOR => Get("UI_ROLE_OPERATOR"),
        TenantRole.STOREKEEPER => Get("UI_ROLE_STOREKEEPER"),
        TenantRole.SALES_MANAGER => Get("UI_ROLE_SALES_MANAGER"),
        TenantRole.AUDITOR => Get("UI_ROLE_AUDITOR"),
        _ => Get("UI_ROLE_EMPLOYEE")
    };
}