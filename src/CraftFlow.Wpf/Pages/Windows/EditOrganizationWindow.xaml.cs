using CraftFlow.SharedKernel.Constants.Endpoints;
using CraftFlow.SharedKernel.Dtos.Identity;
using CraftFlow.SharedKernel.Dtos.Subscription;
using CraftFlow.Wpf.Services;
using System.Windows;

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

        _ = LoadInitialDataAsync();
    }

    private async Task LoadInitialDataAsync()
    {
        await Task.WhenAll(LoadPlansAsync(), LoadPaymentsAsync());
    }

    private async Task LoadPlansAsync()
    {
        var plans = await ApiService.Instance.GetAsync<List<SubscriptionPlanDto>>(
            $"{SubscriptionsConstants.PLANS}?organizationId={_org.Id}");

        if (plans == null || plans.Count == 0) return;

        PlanComboBox.ItemsSource = plans;

        var currentCode = _org.SubscriptionStatus.ToUpperInvariant();
        var matched = plans.FirstOrDefault(p => p.Code.Equals(currentCode, StringComparison.OrdinalIgnoreCase))
                      ?? plans.FirstOrDefault(p => currentCode.Contains(p.Code, StringComparison.OrdinalIgnoreCase));

        PlanComboBox.SelectedItem = matched ?? plans[0];
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
        if (PlanComboBox.SelectedValue is not string planCode || string.IsNullOrWhiteSpace(planCode))
        {
            MessageBox.Show("Выберите тарифный план.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!int.TryParse(AddDaysTextBox.Text, out int addDays)) addDays = 0;
        bool isActive = IsActiveCheckBox.IsChecked ?? true;

        var (isSuccess, contentOrError) = await ApiService.Instance.PostAndReadAsync($"/api/identity/organizations/{_org.Id}/manage", new
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