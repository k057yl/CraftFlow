using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Dtos.Dashboard;
using CraftFlow.SharedKernel.Enums;
using CraftFlow.SharedKernel.Enums.Identity;
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
using CraftFlow.Wpf.Pages.Traceability;
using CraftFlow.Wpf.Pages.WarehouseAndSuppliers;
using CraftFlow.Wpf.Services;
using System.Windows;
using System.Windows.Controls;

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
            Dispatcher.Invoke(UpdateNavigationPermissions);
        };

        _isInitializing = false;

        if (ApiService.Instance.IsAuthenticated)
        {
            UpdateNavigationPermissions();
            MainFrame.Navigate(new DashboardPage(initialSummary));
        }
        else
        {
            NavigateToAuth();
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

        // --- 1. Обзор и Сводка (Дашборд) ---
        SetButtonVisibility(NavDashboardButton, ps.HasPermission(AppPermission.DASHBOARD_VIEW));

        // --- 2. Настройки производства ---
        SetButtonVisibility(NavProductionSettingsButton, ps.HasPermission(AppPermission.RECIPES_MANAGE) || ps.HasPermission(AppPermission.RAW_MATERIALS_MANAGE));

        // --- 3. Склад и Поставщики ---
        SetButtonVisibility(NavWarehouseAndSuppliersButton, ps.HasPermission(AppPermission.PROCUREMENT_VIEW) || ps.HasPermission(AppPermission.SUPPLIERS_MANAGE));

        // --- 4. Расчёт сырья (MRP) ---
        SetButtonVisibility(NavMrpButton, ps.HasPermission(AppPermission.PROCUREMENT_VIEW) || ps.HasPermission(AppPermission.PRODUCTION_VIEW));

        // --- 5. Цеха по подписке ---
        var canViewProduction = ps.HasPermission(AppPermission.PRODUCTION_VIEW) || ps.HasPermission(AppPermission.PRODUCTION_EXECUTE);

        bool isCheeseActive = canViewProduction && (ps.IsSuperAdmin || activeModules.HasFlag(AppModule.CHEESE_MAKING));
        bool isBreweryActive = canViewProduction && (ps.IsSuperAdmin || activeModules.HasFlag(AppModule.BREWERY));
        bool isMeatActive = canViewProduction && (ps.IsSuperAdmin || activeModules.HasFlag(AppModule.MEAT_PROCESSING));
        bool isFermentationActive = canViewProduction && (ps.IsSuperAdmin || activeModules.HasFlag(AppModule.FERMENTATION));

        SetButtonVisibility(NavCheeseMakingButton, isCheeseActive);
        SetButtonVisibility(NavBreweryButton, isBreweryActive);
        SetButtonVisibility(NavMeatProcessingButton, isMeatActive);
        SetButtonVisibility(NavFermentationButton, isFermentationActive);

        // --- 6. Продажи и Клиенты ---
        SetButtonVisibility(NavSalesButton, ps.HasPermission(AppPermission.SALES_VIEW) || ps.HasPermission(AppPermission.SALES_MANAGE));

        // --- 7. История партий (Трассировка) ---
        SetButtonVisibility(NavTraceabilityButton, ps.HasPermission(AppPermission.PRODUCTION_VIEW) || ps.HasPermission(AppPermission.PROCUREMENT_VIEW));

        // --- 8. Журнал событий ---
        SetButtonVisibility(NavAuditButton, ps.HasPermission(AppPermission.SETTINGS_MANAGE));

        // --- 9. Профиль и Админка ---
        SetButtonVisibility(NavProfileButton, ps.IsOwner || ps.IsSuperAdmin);
        SetButtonVisibility(NavAdminButton, ps.IsSuperAdmin);
    }

    private static void SetButtonVisibility(Button? button, bool isVisible)
    {
        if (button != null)
        {
            button.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
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
        ThemeComboBox.SelectedValue = UiConstants.Themes.LIGHT;
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

    // --- ОБРАБОТЧИКИ НАВИГАЦИИ ---
    private void NavDashboard_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new DashboardPage());
    private void NavProductionSettings_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new ProductionSettingsPage());
    private void NavWarehouseAndSuppliers_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new WarehouseAndSuppliersPage());
    private void NavMrp_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new MrpPage());

    private void NavCheeseMaking_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new CheeseMakingPage());
    private void NavBrewery_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new CheeseMakingPage());
    private void NavMeatProcessing_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new CheeseMakingPage());
    private void NavFermentation_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new FermentationPage());

    private void NavSales_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new SalesPage());
    private void NavTraceability_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new TraceabilityPage());
    private void NavAudit_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new AuditPage());
    private void NavProfile_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new ProfilePage());
    private void NavAdmin_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new AdminPage());
    private void NavAuth_Click(object sender, RoutedEventArgs e) => NavigateToAuth();
}