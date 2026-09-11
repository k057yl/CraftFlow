using CraftFlow.Wpf.Models.Auth;
using CraftFlow.Wpf.Services;
using CraftFlow.Wpf.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CraftFlow.Wpf.Pages;

public partial class AdminPage : Page
{
    public AdminPage()
    {
        InitializeComponent();
        _ = LoadOrganizationsAsync();
    }

    private async Task LoadOrganizationsAsync()
    {
        var orgs = await ApiService.Instance.GetAsync<List<OrganizationAdminDto>>("/api/identity/organizations");
        if (orgs != null)
        {
            OrganizationsDataGrid.ItemsSource = orgs;
        }
    }

    private async void OrganizationsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (OrganizationsDataGrid.SelectedItem is OrganizationAdminDto selectedOrg)
        {
            var modal = new EditOrganizationWindow(selectedOrg)
            {
                Owner = Application.Current.MainWindow
            };

            if (modal.ShowDialog() == true)
            {
                await LoadOrganizationsAsync();
            }
        }
    }
}