using CraftFlow.Api.Modules.Analytics;
using CraftFlow.SharedKernel.Dtos.Dashboard;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.Wpf.Services;
using System.Windows;
using System.Windows.Controls;

namespace CraftFlow.Wpf.Pages;

public partial class DashboardPage : Page
{
    public DashboardPage(DashboardSummaryDto? initialSummary = null)
    {
        InitializeComponent();

        FromDatePicker.SelectedDate = DateTime.Now.AddDays(-30);
        ToDatePicker.SelectedDate = DateTime.Now;

        if (!PermissionService.Instance.HasPermission(AppPermission.DASHBOARD_VIEW))
        {
            return;
        }

        if (initialSummary != null)
        {
            BindData(initialSummary);
        }
        else
        {
            Loaded += async (s, e) => await LoadDashboardAsync();
        }
    }

    private async void RefreshDashboard_Click(object sender, RoutedEventArgs e)
    {
        await LoadDashboardAsync();
    }

    private async Task LoadDashboardAsync()
    {
        if (!PermissionService.Instance.HasPermission(AppPermission.DASHBOARD_VIEW)) return;

        var fromDate = FromDatePicker.SelectedDate?.ToString("yyyy-MM-dd");
        var toDate = ToDatePicker.SelectedDate?.ToString("yyyy-MM-dd");

        var url = $"{AnalyticConstants.DASHBOARD}?fromDate={fromDate}&toDate={toDate}";

        var summary = await ApiService.Instance.GetAsync<DashboardSummaryDto>(url);
        if (summary != null)
        {
            BindData(summary);
        }
    }

    private void BindData(DashboardSummaryDto summary)
    {
        ProductsCountTextBlock.Text = $"{summary.TotalProducts} / {summary.TotalRecipes}";
        ActiveBatchesTextBlock.Text = summary.ActiveBatchesCount.ToString();
        RawStockTextBlock.Text = summary.RawMaterialStockQuantity.ToString("N2");
        FinishedStockTextBlock.Text = summary.FinishedProductStockQuantity.ToString("N2");
        TotalRevenueTextBlock.Text = $"${summary.TotalSalesRevenue:N2}";

        ExpiringLotsTextBlock.Text = summary.ExpiringStockLotsCount.ToString();
        OverdueSanitationTextBlock.Text = summary.OverdueSanitationTasksCount.ToString();
        PendingProcurementsTextBlock.Text = summary.PendingProcurementsCount.ToString();
    }
}