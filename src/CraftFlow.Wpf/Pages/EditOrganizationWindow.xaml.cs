using CraftFlow.Wpf.Models.Auth;
using CraftFlow.Wpf.Services;
using System.Windows;
using System.Windows.Controls;

namespace CraftFlow.Wpf.Windows;

public record PaymentDto(Guid Id, DateTime CreatedAtUtc, string PlanCode, int DaysAdded, string Description);

public partial class EditOrganizationWindow : Window
{
    private readonly OrganizationAdminDto _org;

    public EditOrganizationWindow(OrganizationAdminDto org)
    {
        InitializeComponent();
        _org = org;

        OrgNameTextBlock.Text = $"Управление: {org.Name}";
        IsActiveCheckBox.IsChecked = org.IsActive;

        foreach (ComboBoxItem item in PlanComboBox.Items)
        {
            if (item.Tag?.ToString() == org.SubscriptionStatus.ToUpperInvariant())
            {
                PlanComboBox.SelectedItem = item;
                break;
            }
        }

        if (PlanComboBox.SelectedItem == null) PlanComboBox.SelectedIndex = 0;

        _ = LoadPaymentsAsync();
    }

    private async Task LoadPaymentsAsync()
    {
        var payments = await ApiService.Instance.GetAsync<List<PaymentDto>>($"/api/identity/organizations/{_org.Id}/payments");
        if (payments != null)
        {
            PaymentsDataGrid.ItemsSource = payments;
        }
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(AddDaysTextBox.Text, out int addDays)) addDays = 0;

        var selectedItem = (ComboBoxItem)PlanComboBox.SelectedItem;
        string planCode = selectedItem.Tag.ToString()!;
        bool isActive = IsActiveCheckBox.IsChecked ?? true;

        (bool isSuccess, string contentOrError) = await ApiService.Instance.PostAndReadAsync($"/api/identity/organizations/{_org.Id}/manage", new
        {
            PlanCode = planCode,
            AddDays = addDays,
            IsActive = isActive
        });

        if (isSuccess)
        {
            DialogResult = true;
            Close();
        }
        else
        {
            MessageBox.Show(contentOrError, "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}