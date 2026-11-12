using CraftFlow.SharedKernel.Constants.Endpoints;
using CraftFlow.SharedKernel.Dtos.Identity;
using CraftFlow.SharedKernel.Dtos.Subscription;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.Wpf.Services;
using CraftFlow.Wpf.Windows;
using System.Windows;
using System.Windows.Controls;
using static CraftFlow.Wpf.Services.LocalizationService;

namespace CraftFlow.Wpf.Pages;

public partial class ProfilePage : Page
{
    public ProfilePage() : this(null)
    {
    }

    public ProfilePage(object? dummy = null)
    {
        InitializeComponent();
        InitRoleComboBox();
        _ = LoadUserDataAsync();
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

    private async Task LoadUserDataAsync()
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
        DangerZoneBorder.Visibility = (isOwner || isSuperAdmin) ? Visibility.Visible : Visibility.Collapsed;

        var tasks = new List<Task> { LoadSubscriptionOverviewAsync() };
        if (isOwner || isSuperAdmin)
        {
            tasks.Add(LoadTeamUsersAsync());
        }

        await Task.WhenAll(tasks);
    }

    private async Task LoadSubscriptionOverviewAsync()
    {
        var overview = await ApiService.Instance.GetAsync<MySubscriptionOverviewDto>(SubscriptionsConstants.MY_SUBSCRIPTIONS);
        if (overview == null)
        {
            CurrentPlanTitleText.Text = "Не удалось загрузить";
            return;
        }

        SubscriptionStatusBadge.Text = overview.StatusBadge;
        CurrentPlanTitleText.Text = overview.ActivePlansSummary;
        SubscriptionExpiresText.Text = overview.NearestExpirationUtc.HasValue
            ? overview.NearestExpirationUtc.Value.ToLocalTime().ToString("dd.MM.yyyy HH:mm")
            : "Бессрочно";

        PaymentsDataGrid.ItemsSource = overview.Payments;

        ApiService.Instance.ActiveModules = overview.ActiveModules;
        if (Application.Current.MainWindow is MainWindow mainWindow)
        {
            mainWindow.UpdateNavigationPermissions();
        }
    }

    private async void RequestAddonInvoice_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string addonCode)
        {
            button.IsEnabled = false;

            try
            {
                var payload = new { PlanCode = addonCode, Days = 30 };
                var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(SubscriptionsConstants.REQUEST_INVOICE, payload);

                if (isSuccess)
                {
                    var userOrgName = ApiService.Instance.CurrentUser?.FullName ?? "Крафтовик";
                    var requisitesWin = new PaymentRequisitesWindow(addonCode, userOrgName)
                    {
                        Owner = Window.GetWindow(this)
                    };
                    requisitesWin.ShowDialog();
                }
                else
                {
                    MessageBox.Show(contentOrError, Get("UI_CAPTION_ERROR"), MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            finally
            {
                button.IsEnabled = true;
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
            MessageBox.Show(contentOrError.Trim('"'), Get("UI_CAPTION_ERROR"), MessageBoxButton.OK, MessageBoxImage.Error);
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