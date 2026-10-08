using CraftFlow.Api.Modules.Identity.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Dtos.Dashboard;
using CraftFlow.Wpf.Pages;
using CraftFlow.Wpf.Pages.Production;
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
        if (NavAdminKeysButton == null || NavAuthButton == null || NavProfileButton == null || MainMenuPanel == null) return;

        if (!ApiService.Instance.IsAuthenticated)
        {
            MainMenuPanel.Visibility = Visibility.Collapsed;
            NavAdminKeysButton.Visibility = Visibility.Collapsed;
            NavProfileButton.Visibility = Visibility.Collapsed;
            NavAuthButton.Visibility = Visibility.Visible;

            UserProfilePanel.Visibility = Visibility.Collapsed;
            GuestPanel.Visibility = Visibility.Visible;
            return;
        }

        MainMenuPanel.Visibility = Visibility.Visible;

        var currentUser = ApiService.Instance.CurrentUser;
        bool isSuperAdmin = currentUser?.Role == TenantRole.SuperAdmin;
        bool isOwner = currentUser?.Role == TenantRole.Owner;

        GuestPanel.Visibility = Visibility.Collapsed;
        UserProfilePanel.Visibility = Visibility.Visible;

        UserNameTextBlock.Text = currentUser?.FullName ?? string.Empty;
        UserRoleTextBlock.Text = currentUser?.Role switch
        {
            TenantRole.SuperAdmin => LocalizationService.Get(UiConstants.Roles.SUPER_ADMIN_DISPLAY),
            TenantRole.Owner => LocalizationService.Get(UiConstants.Roles.OWNER_DISPLAY),
            TenantRole.Technologist => LocalizationService.Get(UiConstants.Roles.TECHNOLOGIST_DISPLAY),
            TenantRole.Storekeeper => LocalizationService.Get(UiConstants.Roles.STOREKEEPER_DISPLAY),
            TenantRole.SalesManager => LocalizationService.Get(UiConstants.Roles.SALES_MANAGER_DISPLAY),
            _ => LocalizationService.Get(UiConstants.Roles.EMPLOYEE_DISPLAY)
        };

        NavAuthButton.Visibility = Visibility.Collapsed;
        NavAdminKeysButton.Visibility = isSuperAdmin ? Visibility.Visible : Visibility.Collapsed;
        NavProfileButton.Visibility = (isOwner || isSuperAdmin) ? Visibility.Visible : Visibility.Collapsed;
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
            MainFrame.Navigate(Activator.CreateInstance(contentType));
        }
    }

    private void NavRecipes_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new RecipesPage());
    private void NavStorage_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new StoragePage());
    private void NavProcurement_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new ProcurementPage());
    private void NavProduction_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new ProductionPage());
    private void NavSales_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new SalesPage());
    private void NavTraceability_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new TraceabilityPage());
    private void NavMrp_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new MrpPage());
    private void NavAdminKeys_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new AdminPage());
    private void NavAuth_Click(object sender, RoutedEventArgs e) => NavigateToAuth();
    private void NavProfile_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new ProfilePage());
    private void NavAudit_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new AuditPage());
}