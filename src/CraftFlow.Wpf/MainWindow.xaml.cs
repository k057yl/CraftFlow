using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Constants.Endpoints;
using CraftFlow.SharedKernel.Dtos.Dashboard;
using CraftFlow.SharedKernel.Dtos.Subscription;
using CraftFlow.SharedKernel.Enums;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Enums.Subscription;
using CraftFlow.Wpf.Pages.Admin;
using CraftFlow.Wpf.Pages.Audit;
using CraftFlow.Wpf.Pages.Auth;
using CraftFlow.Wpf.Pages.CheeseMaking;
using CraftFlow.Wpf.Pages.Dashboard;
using CraftFlow.Wpf.Pages.Fermentation;
using CraftFlow.Wpf.Pages.Mrp;
using CraftFlow.Wpf.Pages.ProductionSettings;
using CraftFlow.Wpf.Pages.Profile;
using CraftFlow.Wpf.Pages.Sales;
using CraftFlow.Wpf.Pages.Subscriptions;
using CraftFlow.Wpf.Pages.Traceability;
using CraftFlow.Wpf.Pages.WarehouseAndSuppliers;
using CraftFlow.Wpf.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CraftFlow.Wpf;

public partial class MainWindow : Window
{
    private bool _isInitializing = true;

    public MainWindow() : this(null) { }

    public MainWindow(DashboardSummaryDto? initialSummary)
    {
        InitializeComponent();
        Title = UiConstants.Titles.APP_TITLE;

        InitSettingsControls();

        LocalizationService.LanguageChanged += OnLanguageChanged;

        ApiService.Instance.OnAuthStateChanged += () =>
        {
            Dispatcher.Invoke(() =>
            {
                UpdateNavigationPermissions();
                if (ApiService.Instance.IsAuthenticated)
                {
                    _ = LoadSidebarSubscriptionAsync();
                }
            });
        };

        _isInitializing = false;

        if (ApiService.Instance.IsAuthenticated)
        {
            UpdateNavigationPermissions();
            _ = LoadSidebarSubscriptionAsync();

            if (initialSummary != null || HasFeatureAccess(PlanFeatures.ADVANCED_ANALYTICS))
            {
                MainFrame.Navigate(new DashboardPage(initialSummary));
            }
            else
            {
                NavigateToDefaultAvailablePage();
            }
        }
        else
        {
            NavigateToAuth();
        }
    }

    public async Task LoadSidebarSubscriptionAsync()
    {
        if (!ApiService.Instance.IsAuthenticated || PermissionService.Instance.IsSuperAdmin)
        {
            SidebarPlanBorder.Visibility = Visibility.Collapsed;
            return;
        }

        var overview = await ApiService.Instance.GetAsync<MySubscriptionOverviewDto>(SubscriptionsConstants.MY_SUBSCRIPTIONS);
        if (overview == null) return;

        SidebarPlanBorder.Visibility = Visibility.Visible;
        SidebarPlanBadgeText.Text = overview.StatusBadge;
        SidebarPlanTitleText.Text = overview.ActivePlansSummary;
        SidebarPlanDetailsText.Text = overview.NearestExpirationUtc.HasValue
            ? string.Format(LocalizationService.Get("UI_SUB_SIDEBAR_EXPIRES_FMT"), overview.NearestExpirationUtc.Value.ToLocalTime().ToString("dd.MM.yyyy"))
            : LocalizationService.Get("UI_SUB_SIDEBAR_FREE_PERPETUAL");

        var modulesChanged = ApiService.Instance.ActiveModules != overview.ActiveModules;
        var featuresChanged = ApiService.Instance.ActiveFeatures != overview.EffectiveFeatures;

        if (modulesChanged || featuresChanged)
        {
            ApiService.Instance.ActiveModules = overview.ActiveModules;
            ApiService.Instance.ActiveFeatures = overview.EffectiveFeatures;
            UpdateNavigationPermissions();

            if (MainFrame.Content is DashboardPage && !HasFeatureAccess(PlanFeatures.ADVANCED_ANALYTICS))
            {
                NavigateToDefaultAvailablePage();
            }
            else if (MainFrame.Content is MrpPage && !HasFeatureAccess(PlanFeatures.MRP_PLANNING))
            {
                NavigateToDefaultAvailablePage();
            }
        }
    }

    public void NavigateToAuth()
    {
        MainFrame.Navigate(new AuthPage());
        UpdateNavigationPermissions();
    }

    public void UpdateNavigationPermissions()
    {
        if (NavAdminButton == null || NavAuthButton == null || NavProfileButton == null || MainMenuPanel == null) return;

        if (!ApiService.Instance.IsAuthenticated)
        {
            MainMenuPanel.Visibility = Visibility.Collapsed;
            NavAdminButton.Visibility = Visibility.Collapsed;
            NavProfileButton.Visibility = Visibility.Collapsed;
            if (NavAuditButton != null) NavAuditButton.Visibility = Visibility.Collapsed;
            NavAuthButton.Visibility = Visibility.Visible;

            SidebarPlanBorder.Visibility = Visibility.Collapsed;
            UserProfilePanel.Visibility = Visibility.Collapsed;
            GuestPanel.Visibility = Visibility.Visible;
            return;
        }

        MainMenuPanel.Visibility = Visibility.Visible;

        var currentUser = ApiService.Instance.CurrentUser;
        var ps = PermissionService.Instance;
        var activeModules = ApiService.Instance.ActiveModules;

        GuestPanel.Visibility = Visibility.Collapsed;
        UserProfilePanel.Visibility = Visibility.Visible;
        SidebarPlanBorder.Visibility = ps.IsSuperAdmin ? Visibility.Collapsed : Visibility.Visible;

        UserNameTextBlock.Text = currentUser?.FullName ?? string.Empty;
        UserRoleTextBlock.Text = currentUser?.Role switch
        {
            TenantRole.SUPER_ADMIN => LocalizationService.Get(UiConstants.Roles.SUPER_ADMIN_DISPLAY),
            TenantRole.OWNER => LocalizationService.Get(UiConstants.Roles.OWNER_DISPLAY),
            TenantRole.TECHNOLOGIST => LocalizationService.Get(UiConstants.Roles.TECHNOLOGIST_DISPLAY),
            TenantRole.STOREKEEPER => LocalizationService.Get(UiConstants.Roles.STOREKEEPER_DISPLAY),
            TenantRole.SALES_MANAGER => LocalizationService.Get(UiConstants.Roles.SALES_MANAGER_DISPLAY),
            _ => LocalizationService.Get(UiConstants.Roles.EMPLOYEE_DISPLAY)
        };

        NavAuthButton.Visibility = Visibility.Collapsed;

        var canViewDashboard = ps.HasPermission(AppPermission.DASHBOARD_VIEW) && HasFeatureAccess(PlanFeatures.ADVANCED_ANALYTICS);
        var canViewMrp = (ps.HasPermission(AppPermission.PROCUREMENT_VIEW) || ps.HasPermission(AppPermission.PRODUCTION_VIEW))
                         && HasFeatureAccess(PlanFeatures.MRP_PLANNING);

        SetButtonVisibility(NavDashboardButton, canViewDashboard);
        SetButtonVisibility(NavProductionSettingsButton, ps.HasPermission(AppPermission.RECIPES_MANAGE) || ps.HasPermission(AppPermission.RAW_MATERIALS_MANAGE));
        SetButtonVisibility(NavWarehouseAndSuppliersButton, ps.HasPermission(AppPermission.PROCUREMENT_VIEW) || ps.HasPermission(AppPermission.SUPPLIERS_MANAGE));
        SetButtonVisibility(NavMrpButton, canViewMrp);

        var canViewProduction = ps.HasPermission(AppPermission.PRODUCTION_VIEW) || ps.HasPermission(AppPermission.PRODUCTION_EXECUTE);

        ConfigureModuleButton(NavCheeseMakingButton, BadgeCheese, canViewProduction, ps.IsSuperAdmin || activeModules.HasFlag(AppModule.CHEESE_MAKING));
        ConfigureModuleButton(NavBreweryButton, BadgeBrewery, canViewProduction, ps.IsSuperAdmin || activeModules.HasFlag(AppModule.BREWERY));
        ConfigureModuleButton(NavMeatProcessingButton, BadgeMeat, canViewProduction, ps.IsSuperAdmin || activeModules.HasFlag(AppModule.MEAT_PROCESSING));
        ConfigureModuleButton(NavFermentationButton, BadgeFermentation, canViewProduction, ps.IsSuperAdmin || activeModules.HasFlag(AppModule.FERMENTATION));

        SetButtonVisibility(NavSalesButton, ps.HasPermission(AppPermission.SALES_VIEW) || ps.HasPermission(AppPermission.SALES_MANAGE));
        SetButtonVisibility(NavTraceabilityButton, ps.HasPermission(AppPermission.PRODUCTION_VIEW) || ps.HasPermission(AppPermission.PROCUREMENT_VIEW));
        SetButtonVisibility(NavAuditButton, ps.HasPermission(AppPermission.SETTINGS_MANAGE));

        SetButtonVisibility(NavProfileButton, ps.IsOwner || ps.IsSuperAdmin);
        SetButtonVisibility(NavAdminButton, ps.IsSuperAdmin);
    }

    public void NavigateToDefaultAvailablePage()
    {
        var ps = PermissionService.Instance;

        if (ps.HasPermission(AppPermission.DASHBOARD_VIEW) && HasFeatureAccess(PlanFeatures.ADVANCED_ANALYTICS))
        {
            MainFrame.Navigate(new DashboardPage());
            return;
        }

        if (ps.HasPermission(AppPermission.PRODUCTION_VIEW) || ps.HasPermission(AppPermission.PRODUCTION_EXECUTE))
        {
            if (HasModuleAccess(AppModule.CHEESE_MAKING))
            {
                MainFrame.Navigate(new CheeseMakingPage());
                return;
            }
            if (HasModuleAccess(AppModule.FERMENTATION))
            {
                MainFrame.Navigate(new FermentationPage());
                return;
            }
        }

        if (ps.HasPermission(AppPermission.RECIPES_MANAGE) || ps.HasPermission(AppPermission.RAW_MATERIALS_MANAGE))
        {
            MainFrame.Navigate(new ProductionSettingsPage());
            return;
        }

        if (ps.IsOwner || ps.IsSuperAdmin)
        {
            MainFrame.Navigate(new ProfilePage());
        }
    }

    private static void ConfigureModuleButton(Button? button, Border? badge, bool canViewProduction, bool isConnected)
    {
        if (button == null) return;

        if (!canViewProduction)
        {
            button.Visibility = Visibility.Collapsed;
            return;
        }

        button.Visibility = Visibility.Visible;
        button.Opacity = isConnected ? 1.0 : 0.65;
        if (badge != null)
        {
            badge.Visibility = isConnected ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    private static void SetButtonVisibility(Button? button, bool isVisible)
    {
        if (button != null)
        {
            button.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private bool HasModuleAccess(AppModule module) =>
        PermissionService.Instance.IsSuperAdmin || ApiService.Instance.ActiveModules.HasFlag(module);

    private bool HasFeatureAccess(PlanFeatures feature) =>
        PermissionService.Instance.IsSuperAdmin || ApiService.Instance.ActiveFeatures.HasFlag(feature);

    private void SidebarPlan_Click(object sender, MouseButtonEventArgs e)
    {
        if (PermissionService.Instance.IsOwner || PermissionService.Instance.IsSuperAdmin)
        {
            MainFrame.Navigate(new ProfilePage());
        }
    }

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        ApiService.Instance.ClearAuthToken();
        ApiService.Instance.ClearTenantHeader();
        NavigateToAuth();
    }

    private void InitSettingsControls()
    {
        LanguageComboBox.ItemsSource = new[]
        {
            new { Code = UiConstants.Cultures.RU, Display = LocalizationService.Get("LANG_RU_DISPLAY") },
            new { Code = UiConstants.Cultures.UA, Display = LocalizationService.Get("LANG_UA_DISPLAY") },
            new { Code = UiConstants.Cultures.EN, Display = LocalizationService.Get("LANG_EN_DISPLAY") }
        };
        LanguageComboBox.DisplayMemberPath = "Display";
        LanguageComboBox.SelectedValuePath = "Code";
        LanguageComboBox.SelectedValue = LocalizationService.CurrentCulture.Name;

        ThemeComboBox.ItemsSource = new[]
        {
            new { Code = UiConstants.Themes.LIGHT, Display = LocalizationService.Get("THEME_LIGHT_DISPLAY") },
            new { Code = UiConstants.Themes.DARK, Display = LocalizationService.Get("THEME_DARK_DISPLAY") }
        };
        ThemeComboBox.DisplayMemberPath = "Display";
        ThemeComboBox.SelectedValuePath = "Code";
        ThemeComboBox.SelectedValue = ThemeService.CurrentTheme;
    }

    private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing || LanguageComboBox.SelectedValue is not string cultureCode) return;
        LocalizationService.SetCulture(cultureCode);
    }

    private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing || ThemeComboBox.SelectedValue is not string themeName) return;
        ThemeService.SetTheme(themeName);
        RefreshUiContent();
    }

    private void OnLanguageChanged()
    {
        _isInitializing = true;
        InitSettingsControls();
        UpdateNavigationPermissions();
        if (ApiService.Instance.IsAuthenticated)
        {
            _ = LoadSidebarSubscriptionAsync();
        }
        _isInitializing = false;
        RefreshUiContent();
    }

    private void RefreshUiContent()
    {
        if (MainFrame.Content is object currentContent)
        {
            var contentType = currentContent.GetType();
            var defaultConstructor = contentType.GetConstructor(Type.EmptyTypes);
            if (defaultConstructor != null)
            {
                MainFrame.Navigate(Activator.CreateInstance(contentType));
            }
            else
            {
                var firstConstructor = contentType.GetConstructors().FirstOrDefault();
                if (firstConstructor != null)
                {
                    var args = new object?[firstConstructor.GetParameters().Length];
                    MainFrame.Navigate(Activator.CreateInstance(contentType, args));
                }
            }
        }
    }

    private void NavDashboard_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new DashboardPage());
    private void NavProductionSettings_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new ProductionSettingsPage());
    private void NavWarehouseAndSuppliers_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new WarehouseAndSuppliersPage());
    private void NavMrp_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new MrpPage());

    private void NavCheeseMaking_Click(object sender, RoutedEventArgs e) =>
        MainFrame.Navigate(HasModuleAccess(AppModule.CHEESE_MAKING)
            ? new CheeseMakingPage()
            : new ModulePreviewPage(AppModule.CHEESE_MAKING));

    private void NavBrewery_Click(object sender, RoutedEventArgs e) =>
        MainFrame.Navigate(HasModuleAccess(AppModule.BREWERY)
            ? new CheeseMakingPage()
            : new ModulePreviewPage(AppModule.BREWERY));

    private void NavMeatProcessing_Click(object sender, RoutedEventArgs e) =>
        MainFrame.Navigate(HasModuleAccess(AppModule.MEAT_PROCESSING)
            ? new CheeseMakingPage()
            : new ModulePreviewPage(AppModule.MEAT_PROCESSING));

    private void NavFermentation_Click(object sender, RoutedEventArgs e) =>
        MainFrame.Navigate(HasModuleAccess(AppModule.FERMENTATION)
            ? new FermentationPage()
            : new ModulePreviewPage(AppModule.FERMENTATION));

    private void NavSales_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new SalesPage());
    private void NavTraceability_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new TraceabilityPage());
    private void NavAudit_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new AuditPage());
    private void NavProfile_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new ProfilePage());
    private void NavAdmin_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new AdminPage());
    private void NavAuth_Click(object sender, RoutedEventArgs e) => NavigateToAuth();
}