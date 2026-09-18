using CraftFlow.Api.Modules.Identity.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Pages;
using CraftFlow.Wpf.Services;
using System.Windows;
using System.Windows.Controls;

namespace CraftFlow.Wpf;

public partial class MainWindow : Window
{
    private const string LANG_RU = "ru-RU";
    private const string LANG_UA = "uk-UA";
    private const string LANG_EN = "en-US";

    private const string THEME_LIGHT = "Light";
    private const string THEME_DARK = "Dark";

    private bool _isInitializing = true;

    public MainWindow() : this(null) { }

    public MainWindow(DashboardSummaryDto? initialSummary)
    {
        InitializeComponent();
        Title = UiConstants.Titles.APP_TITLE;

        InitSettingsControls();
        LocalizationService.LanguageChanged += RefreshUiContent;

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

        GuestPanel.Visibility = Visibility.Collapsed;
        UserProfilePanel.Visibility = Visibility.Visible;

        UserNameTextBlock.Text = currentUser?.FullName ?? string.Empty;
        UserRoleTextBlock.Text = currentUser?.Role switch
        {
            TenantRole.SuperAdmin => "SuperAdmin (SaaS)",
            TenantRole.Owner => "Владелец",
            TenantRole.Technologist => "Технолог",
            TenantRole.Storekeeper => "Кладовщик",
            TenantRole.SalesManager => "Менеджер продаж",
            _ => "Сотрудник"
        };

        NavAuthButton.Visibility = Visibility.Collapsed;

        bool canViewProfile = currentUser?.Role == TenantRole.Owner || isSuperAdmin;
        NavProfileButton.Visibility = canViewProfile ? Visibility.Visible : Visibility.Collapsed;
        NavAdminKeysButton.Visibility = isSuperAdmin ? Visibility.Visible : Visibility.Collapsed;
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
            new { Code = LANG_RU, Display = "RU" },
            new { Code = LANG_UA, Display = "UA" },
            new { Code = LANG_EN, Display = "EN" }
        };
        LanguageComboBox.DisplayMemberPath = "Display";
        LanguageComboBox.SelectedValuePath = "Code";
        LanguageComboBox.SelectedValue = LocalizationService.CurrentCulture.Name;

        ThemeComboBox.ItemsSource = new[]
        {
            new { Code = THEME_LIGHT, Display = "☀️ Light" },
            new { Code = THEME_DARK, Display = "🌙 Dark" }
        };
        ThemeComboBox.DisplayMemberPath = "Display";
        ThemeComboBox.SelectedValuePath = "Code";
        ThemeComboBox.SelectedValue = THEME_LIGHT;
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

    private void RefreshUiContent()
    {
        if (MainFrame.Content is object currentContent)
        {
            var contentType = currentContent.GetType();
            MainFrame.Navigate(Activator.CreateInstance(contentType));
        }
    }

    private void NavDirectory_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new CatalogPage());
    private void NavInventory_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new InventoryPage());
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