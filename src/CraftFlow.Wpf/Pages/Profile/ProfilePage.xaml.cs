using CraftFlow.SharedKernel.Constants.Endpoints;
using CraftFlow.SharedKernel.Dtos.Identity;
using CraftFlow.SharedKernel.Dtos.Subscription;
using CraftFlow.SharedKernel.Enums;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.Wpf.Services;
using CraftFlow.Wpf.Windows;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static CraftFlow.Wpf.Services.LocalizationService;

namespace CraftFlow.Wpf.Pages.Profile;

public class ModuleTabVm
{
    public AppModule Module { get; init; }
    public string Prefix { get; init; } = string.Empty;
    public string ShortTitle { get; init; } = string.Empty;
    public string FullTitle { get; init; } = string.Empty;
    public bool IsConnected { get; init; }
    public string CurrentTier { get; init; } = string.Empty;
    public DateTime? ExpiresAtUtc { get; init; }
    public bool IsSelected { get; set; }

    public Brush StatusDotBrush => IsConnected
        ? new SolidColorBrush(Color.FromRgb(22, 163, 74))
        : new SolidColorBrush(Color.FromRgb(148, 163, 184));

    public Brush TabBackground => IsSelected
        ? new SolidColorBrush(Color.FromArgb(25, 37, 99, 235))
        : Brushes.Transparent;

    public Brush TabBorder => IsSelected
        ? new SolidColorBrush(Color.FromRgb(37, 99, 235))
        : (Application.Current.TryFindResource("BorderBrush") as Brush ?? Brushes.Gray);

    public Brush TabForeground => IsSelected
        ? new SolidColorBrush(Color.FromRgb(37, 99, 235))
        : (Application.Current.TryFindResource("AppForegroundBrush") as Brush ?? Brushes.Black);
}

public class TierCardVm
{
    public string TierCode { get; init; } = string.Empty;
    public string PlanCode { get; init; } = string.Empty;
    public string TierTitle { get; init; } = string.Empty;
    public string Subtitle { get; init; } = string.Empty;

    public string BatchesText { get; init; } = string.Empty;
    public string WarehousesText { get; init; } = string.Empty;
    public string ChambersText { get; init; } = string.Empty;
    public string UsersText { get; init; } = string.Empty;
    public string AuditText { get; init; } = string.Empty;

    public bool HasPdfExport { get; init; }
    public bool HasExcelExport { get; init; }
    public bool HasAnalytics { get; init; }
    public bool HasMrp { get; init; }

    public bool IsCurrent { get; init; }
    public bool IsFreeTier { get; init; }
    public bool IsModuleConnected { get; init; }

    private static Brush ActiveFeatureBrush =>
        Application.Current.TryFindResource("AppForegroundBrush") as Brush ?? Brushes.Black;

    private static Brush InactiveFeatureBrush =>
        new SolidColorBrush(Color.FromRgb(148, 163, 184));

    public string PdfExportText => HasPdfExport ? Get("UI_SUB_FEAT_PDF_YES") : Get("UI_SUB_FEAT_PDF_NO");
    public Brush PdfExportBrush => HasPdfExport ? ActiveFeatureBrush : InactiveFeatureBrush;

    public string ExcelExportText => HasExcelExport ? Get("UI_SUB_FEAT_EXCEL_YES") : Get("UI_SUB_FEAT_EXCEL_NO");
    public Brush ExcelExportBrush => HasExcelExport ? ActiveFeatureBrush : InactiveFeatureBrush;

    public string AnalyticsText => HasAnalytics ? Get("UI_SUB_FEAT_ANALYTICS_YES") : Get("UI_SUB_FEAT_ANALYTICS_NO");
    public Brush AnalyticsBrush => HasAnalytics ? ActiveFeatureBrush : InactiveFeatureBrush;

    public string MrpText => HasMrp ? Get("UI_SUB_FEAT_MRP_YES") : Get("UI_SUB_FEAT_MRP_NO");
    public Brush MrpBrush => HasMrp ? ActiveFeatureBrush : InactiveFeatureBrush;

    public Visibility CurrentBadgeVisibility => IsCurrent ? Visibility.Visible : Visibility.Collapsed;

    public Brush CardBackground => IsCurrent
        ? new SolidColorBrush(Color.FromArgb(18, 22, 163, 74))
        : (Application.Current.TryFindResource("AppBackgroundBrush") as Brush ?? Brushes.White);

    public Brush CardBorder => IsCurrent
        ? new SolidColorBrush(Color.FromRgb(22, 163, 74))
        : (Application.Current.TryFindResource("BorderBrush") as Brush ?? Brushes.LightGray);

    public bool IsActionEnabled => !IsFreeTier;

    public string ActionButtonText
    {
        get
        {
            if (IsFreeTier)
                return IsCurrent ? Get("UI_SUB_BTN_ACTIVE") : Get("UI_SUB_BTN_BASIC");

            if (IsCurrent)
                return Get("UI_SUB_BTN_RENEW");

            if (IsModuleConnected)
                return Get("UI_SUB_BTN_SWITCH");

            return Get("UI_SUB_BTN_CONNECT");
        }
    }
}

public partial class ProfilePage : Page
{
    private List<ModuleTabVm> _moduleTabs = [];
    private AppModule _selectedModule = AppModule.NONE;

    public ProfilePage() : this(null)
    {
    }

    public ProfilePage(object? initialModule = null)
    {
        InitializeComponent();

        if (initialModule is AppModule mod && mod != AppModule.NONE)
        {
            _selectedModule = mod;
        }

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
            CurrentPlanTitleText.Text = Get("UI_SUB_PROFILE_LOAD_FAILED");
            return;
        }

        SubscriptionStatusBadge.Text = overview.StatusBadge;
        CurrentPlanTitleText.Text = overview.ActivePlansSummary;
        SubscriptionExpiresText.Text = overview.NearestExpirationUtc.HasValue
            ? overview.NearestExpirationUtc.Value.ToLocalTime().ToString("dd.MM.yyyy HH:mm")
            : Get("UI_SUB_PROFILE_PERPETUAL");

        PaymentsDataGrid.ItemsSource = overview.Payments;

        ApiService.Instance.ActiveModules = overview.ActiveModules;
        if (Application.Current.MainWindow is MainWindow mainWindow)
        {
            mainWindow.UpdateNavigationPermissions();
            _ = mainWindow.LoadSidebarSubscriptionAsync();
        }

        _moduleTabs = overview.Modules.Select(mod =>
        {
            var (prefix, shortTitle, fullTitle) = mod.Module switch
            {
                AppModule.CHEESE_MAKING => ("CHEESE", Get("UI_SUB_TAB_CHEESE_SHORT"), Get("UI_SUB_TAB_CHEESE_FULL")),
                AppModule.BREWERY => ("BEER", Get("UI_SUB_TAB_BREWERY_SHORT"), Get("UI_SUB_TAB_BREWERY_FULL")),
                AppModule.MEAT_PROCESSING => ("MEAT", Get("UI_SUB_TAB_MEAT_SHORT"), Get("UI_SUB_TAB_MEAT_FULL")),
                AppModule.FERMENTATION => ("FERMENTATION", Get("UI_SUB_TAB_FERM_SHORT"), Get("UI_SUB_TAB_FERM_FULL")),
                _ => ("", mod.Module.ToString(), mod.Module.ToString())
            };

            return new ModuleTabVm
            {
                Module = mod.Module,
                Prefix = prefix,
                ShortTitle = shortTitle,
                FullTitle = fullTitle,
                IsConnected = mod.IsConnected,
                CurrentTier = mod.TierSuffix,
                ExpiresAtUtc = mod.ExpiresAtUtc
            };
        }).ToList();

        if (_selectedModule == AppModule.NONE)
        {
            var firstConnected = _moduleTabs.FirstOrDefault(m => m.IsConnected);
            _selectedModule = firstConnected?.Module ?? _moduleTabs.FirstOrDefault()?.Module ?? AppModule.CHEESE_MAKING;
        }

        RenderModuleShowcase();
    }

    private void SelectModuleTab_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is AppModule module)
        {
            _selectedModule = module;
            RenderModuleShowcase();
        }
    }

    private void RenderModuleShowcase()
    {
        foreach (var tab in _moduleTabs)
        {
            tab.IsSelected = tab.Module == _selectedModule;
        }

        ModuleTabsControl.ItemsSource = null;
        ModuleTabsControl.ItemsSource = _moduleTabs;

        var activeTab = _moduleTabs.FirstOrDefault(t => t.Module == _selectedModule);
        if (activeTab == null) return;

        SelectedModuleTitleText.Text = activeTab.FullTitle;

        if (!activeTab.IsConnected)
        {
            SelectedModuleStatusText.Text = Get("UI_SUB_STATUS_NOT_CONNECTED");
            SelectedModuleStatusText.Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139));
        }
        else
        {
            var tierLabel = activeTab.CurrentTier switch
            {
                "FREE" => Get("UI_SUB_TIER_FREE_FULL"),
                "STARTER" => Get("UI_SUB_TIER_STARTER_NAME"),
                "STANDARD" => Get("UI_SUB_TIER_STANDARD_NAME"),
                "PRO" => Get("UI_SUB_TIER_PRO_NAME"),
                _ => Get("UI_SUB_TIER_ACTIVE_DEFAULT")
            };
            var expiry = activeTab.ExpiresAtUtc.HasValue
                ? string.Format(Get("UI_SUB_STATUS_UNTIL_FMT"), activeTab.ExpiresAtUtc.Value.ToLocalTime().ToString("dd.MM.yyyy"))
                : Get("UI_SUB_STATUS_PERPETUAL_LOWER");

            SelectedModuleStatusText.Text = string.Format(Get("UI_SUB_STATUS_CONNECTED_FMT"), tierLabel, expiry);
            SelectedModuleStatusText.Foreground = new SolidColorBrush(Color.FromRgb(22, 163, 74));
        }

        var cards = new ObservableCollection<TierCardVm>
        {
            new()
            {
                TierCode = "FREE",
                PlanCode = $"{activeTab.Prefix}_FREE",
                TierTitle = Get("UI_SUB_TIER_FREE_NAME"),
                Subtitle = Get("UI_SUB_TIER_FREE_SUB"),
                BatchesText = string.Format(Get("UI_SUB_LIMIT_BATCHES_FMT"), 8),
                WarehousesText = string.Format(Get("UI_SUB_LIMIT_WAREHOUSES_FMT"), 2),
                ChambersText = Get("UI_SUB_LIMIT_CHAMBERS_ONE"),
                UsersText = Get("UI_SUB_LIMIT_USERS_ONE"),
                AuditText = string.Format(Get("UI_SUB_LIMIT_AUDIT_DAYS_FMT"), 7),
                HasPdfExport = false,
                HasExcelExport = false,
                HasAnalytics = false,
                HasMrp = false,
                IsFreeTier = true,
                IsModuleConnected = activeTab.IsConnected,
                IsCurrent = activeTab.IsConnected && activeTab.CurrentTier == "FREE"
            },
            new()
            {
                TierCode = "STARTER",
                PlanCode = $"{activeTab.Prefix}_STARTER",
                TierTitle = Get("UI_SUB_TIER_STARTER_NAME"),
                Subtitle = Get("UI_SUB_TIER_STARTER_SUB"),
                BatchesText = string.Format(Get("UI_SUB_LIMIT_BATCHES_FMT"), 20),
                WarehousesText = string.Format(Get("UI_SUB_LIMIT_WAREHOUSES_FMT"), 3),
                ChambersText = string.Format(Get("UI_SUB_LIMIT_CHAMBERS_FMT"), 2),
                UsersText = string.Format(Get("UI_SUB_LIMIT_USERS_FMT"), 2),
                AuditText = string.Format(Get("UI_SUB_LIMIT_AUDIT_DAYS_FMT"), 30),
                HasPdfExport = true,
                HasExcelExport = false,
                HasAnalytics = false,
                HasMrp = false,
                IsFreeTier = false,
                IsModuleConnected = activeTab.IsConnected,
                IsCurrent = activeTab.IsConnected && activeTab.CurrentTier == "STARTER"
            },
            new()
            {
                TierCode = "STANDARD",
                PlanCode = $"{activeTab.Prefix}_STANDARD",
                TierTitle = Get("UI_SUB_TIER_STANDARD_NAME"),
                Subtitle = Get("UI_SUB_TIER_STANDARD_SUB"),
                BatchesText = string.Format(Get("UI_SUB_LIMIT_BATCHES_FMT"), 60),
                WarehousesText = string.Format(Get("UI_SUB_LIMIT_WAREHOUSES_FMT"), 5),
                ChambersText = string.Format(Get("UI_SUB_LIMIT_CHAMBERS_FMT"), 5),
                UsersText = string.Format(Get("UI_SUB_LIMIT_USERS_FMT"), 5),
                AuditText = string.Format(Get("UI_SUB_LIMIT_AUDIT_DAYS_FMT"), 180),
                HasPdfExport = true,
                HasExcelExport = true,
                HasAnalytics = true,
                HasMrp = true,
                IsFreeTier = false,
                IsModuleConnected = activeTab.IsConnected,
                IsCurrent = activeTab.IsConnected && activeTab.CurrentTier == "STANDARD"
            },
            new()
            {
                TierCode = "PRO",
                PlanCode = $"{activeTab.Prefix}_PRO",
                TierTitle = Get("UI_SUB_TIER_PRO_NAME"),
                Subtitle = Get("UI_SUB_TIER_PRO_SUB"),
                BatchesText = string.Format(Get("UI_SUB_LIMIT_BATCHES_FMT"), 200),
                WarehousesText = string.Format(Get("UI_SUB_LIMIT_WAREHOUSES_FMT"), 10),
                ChambersText = string.Format(Get("UI_SUB_LIMIT_CHAMBERS_FMT"), 12),
                UsersText = string.Format(Get("UI_SUB_LIMIT_USERS_FMT"), 10),
                AuditText = string.Format(Get("UI_SUB_LIMIT_AUDIT_YEARS_FMT"), 10),
                HasPdfExport = true,
                HasExcelExport = true,
                HasAnalytics = true,
                HasMrp = true,
                IsFreeTier = false,
                IsModuleConnected = activeTab.IsConnected,
                IsCurrent = activeTab.IsConnected && activeTab.CurrentTier == "PRO"
            }
        };

        TierCardsControl.ItemsSource = cards;
    }

    private async void TierCardAction_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string planCode && !string.IsNullOrWhiteSpace(planCode))
        {
            button.IsEnabled = false;

            try
            {
                var payload = new { PlanCode = planCode, Days = 30 };
                var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync(SubscriptionsConstants.REQUEST_INVOICE, payload);

                if (isSuccess)
                {
                    var userOrgName = ApiService.Instance.CurrentUser?.FullName ?? Get("UI_SUB_DEFAULT_USER_NAME");
                    var requisitesWin = new PaymentRequisitesWindow(planCode, userOrgName)
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