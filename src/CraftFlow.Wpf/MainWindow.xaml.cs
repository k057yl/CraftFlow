using System;
using System.Windows;
using System.Windows.Controls;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.Wpf.Pages;
using CraftFlow.Wpf.Services;

namespace CraftFlow.Wpf;

public partial class MainWindow : Window
{
    private bool _isInitializing = true;

    public MainWindow()
    {
        InitializeComponent();
        Title = UiConstants.Titles.APP_TITLE;

        InitSettingsControls();

        LocalizationService.LanguageChanged += RefreshUiContent;

        if (!ApiService.Instance.IsAuthenticated)
        {
            NavigateToAuth();
        }
        else
        {
            MainFrame.Navigate(new ProductionPage());
        }

        _isInitializing = false;
    }

    public void NavigateToAuth()
    {
        MainFrame.Navigate(new AuthPage());
    }

    private void InitSettingsControls()
    {
        LanguageComboBox.ItemsSource = new[]
        {
            new { Code = "ru-RU", Display = "RU" },
            new { Code = "uk-UA", Display = "UA" },
            new { Code = "en-US", Display = "EN" }
        };
        LanguageComboBox.DisplayMemberPath = "Display";
        LanguageComboBox.SelectedValuePath = "Code";
        LanguageComboBox.SelectedValue = LocalizationService.CurrentCulture.Name;

        ThemeComboBox.ItemsSource = new[]
        {
            new { Code = "Light", Display = "☀️ Светлая" },
            new { Code = "Dark", Display = "🌙 Тёмная" }
        };
        ThemeComboBox.DisplayMemberPath = "Display";
        ThemeComboBox.SelectedValuePath = "Code";
        ThemeComboBox.SelectedValue = "Light";
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
        if (MainFrame.Content is Page currentPage)
        {
            var pageType = currentPage.GetType();
            MainFrame.Navigate(Activator.CreateInstance(pageType));
        }
    }

    private void NavDirectory_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new CatalogPage());
    private void NavInventory_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new InventoryPage());
    private void NavProcurement_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new ProcurementPage());
    private void NavProduction_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new ProductionPage());
    private void NavSales_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new SalesPage());
    private void NavTraceability_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new TraceabilityPage());
    private void NavMrp_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new MrpPage());
    private void NavAuth_Click(object sender, RoutedEventArgs e) => NavigateToAuth();
    private void NavAudit_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new AuditPage());
}