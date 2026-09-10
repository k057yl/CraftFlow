using System;
using System.Windows;
using System.Windows.Controls;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.Wpf.Pages;
using CraftFlow.Wpf.Services;

namespace CraftFlow.Wpf;

public partial class MainWindow : Window
{
    private const string LANG_RU = "ru-RU";
    private const string LANG_UA = "uk-UA";
    private const string LANG_EN = "en-US";

    private const string THEME_LIGHT = "Light";
    private const string THEME_DARK = "Dark";

    private bool _isInitializing = true;

    public MainWindow()
    {
        InitializeComponent();
        Title = UiConstants.Titles.APP_TITLE;

        InitSettingsControls();

        LocalizationService.LanguageChanged += RefreshUiContent;

        if (ApiService.Instance.IsAuthenticated)
        {
            UpdateNavigationPermissions();
            MainFrame.Navigate(new DashboardPage());
        }
        else
        {
            NavigateToAuth();
        }

        _isInitializing = false;
    }

    public void NavigateToAuth()
    {
        MainFrame.Navigate(new AuthPage());
        UpdateNavigationPermissions();
    }

    public void UpdateNavigationPermissions()
    {
        if (NavTenantKeysButton == null || NavAdminKeysButton == null) return;

        if (!ApiService.Instance.IsAuthenticated)
        {
            NavTenantKeysButton.Visibility = Visibility.Collapsed;
            NavAdminKeysButton.Visibility = Visibility.Collapsed;

            UserProfilePanel.Visibility = Visibility.Collapsed;
            GuestPanel.Visibility = Visibility.Visible;
            return;
        }

        var currentUser = ApiService.Instance.CurrentUser;
        bool isAdmin = currentUser?.IsAdmin ?? false;

        GuestPanel.Visibility = Visibility.Collapsed;
        UserProfilePanel.Visibility = Visibility.Visible;

        UserNameTextBlock.Text = currentUser?.FullName ?? string.Empty;
        UserRoleTextBlock.Text = isAdmin ? AuthConstants.Roles.ADMIN : AuthConstants.Roles.USER;

        if (isAdmin)
        {
            NavAdminKeysButton.Visibility = Visibility.Visible;
            NavTenantKeysButton.Visibility = Visibility.Collapsed;
        }
        else
        {
            NavTenantKeysButton.Visibility = Visibility.Visible;
            NavAdminKeysButton.Visibility = Visibility.Collapsed;
        }
    }

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        ApiService.Instance.ClearAuthToken();
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
    private void NavTenantKeys_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new TenantKeysPage());
    private void NavAdminKeys_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new AdminKeysPage());
    private void NavAuth_Click(object sender, RoutedEventArgs e) => NavigateToAuth();
    private void NavAudit_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new AuditPage());
}