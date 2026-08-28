using System.Windows;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.Wpf.Pages;

namespace CraftFlow.Wpf;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Title = UiConstants.Titles.APP_TITLE;
        MainFrame.Navigate(new DashboardPage());
    }

    private void NavDashboard_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new DashboardPage());
    }

    private void NavCatalog_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new CatalogPage());
    }

    private void NavInventory_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new InventoryPage());
    }

    private void NavProduction_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new ProductionPage());
    }

    private void NavSales_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new SalesPage());
    }

    private void NavAuth_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new AuthPage());
    }

    private void NavAudit_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new AuditPage());
    }
}