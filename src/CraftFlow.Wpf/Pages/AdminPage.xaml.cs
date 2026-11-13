using CraftFlow.SharedKernel.Constants.Endpoints;
using CraftFlow.SharedKernel.Dtos.Identity;
using CraftFlow.SharedKernel.Dtos.Subscription;
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

        if (!PermissionService.Instance.IsSuperAdmin)
        {
            MessageBox.Show("Доступ запрещен. Раздел только для администратора системы.", "Ошибка доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        await Task.WhenAll(LoadOrganizationsAsync(), LoadPendingInvoicesAsync());
    }

    private async Task LoadOrganizationsAsync()
    {
        var orgs = await ApiService.Instance.GetAsync<List<OrganizationAdminDto>>(IdentityConstants.ORGANIZATIONS);
        if (orgs != null)
        {
            OrganizationsDataGrid.ItemsSource = orgs;
        }
    }

    private async Task LoadPendingInvoicesAsync()
    {
        var invoices = await ApiService.Instance.GetAsync<List<PendingInvoiceDto>>("api/subscriptions/invoices/pending");
        if (invoices != null)
        {
            PendingInvoicesDataGrid.ItemsSource = invoices;
        }
    }

    private async void ApproveInvoice_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Guid invoiceId)
        {
            button.IsEnabled = false;

            var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync($"api/subscriptions/invoices/{invoiceId}/approve", new { });

            if (isSuccess)
            {
                MessageBox.Show("Оплата подтверждена! Тариф/Модуль сыроварни успешно обновлен.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadDataAsync();
            }
            else
            {
                MessageBox.Show(contentOrError, "Ошибка активации", MessageBoxButton.OK, MessageBoxImage.Error);
                button.IsEnabled = true;
            }
        }
    }

    private async void RejectInvoice_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Guid invoiceId)
        {
            var confirm = MessageBox.Show(
                "Отклонить заявку на оплату? Она будет удалена из списка входящих.",
                "Подтверждение отклонения",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            button.IsEnabled = false;

            var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync($"api/subscriptions/invoices/{invoiceId}/reject", new { });

            if (isSuccess)
            {
                await LoadDataAsync();
            }
            else
            {
                MessageBox.Show(contentOrError, "Ошибка отклонения", MessageBoxButton.OK, MessageBoxImage.Error);
                button.IsEnabled = true;
            }
        }
    }

    private async void OrganizationsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (!PermissionService.Instance.IsSuperAdmin) return;

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