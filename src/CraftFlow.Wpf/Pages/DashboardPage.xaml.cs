using CraftFlow.Api.Modules.Analytics;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.Wpf.Models;
using CraftFlow.Wpf.Services;
using System.Windows.Controls;

namespace CraftFlow.Wpf.Pages;

public partial class DashboardPage : Page
{
    public DashboardPage()
    {
        InitializeComponent();
        Loaded += async (s, e) => await LoadDashboardAsync();
    }

    private async Task LoadDashboardAsync()
    {
        var summary = await ApiService.Instance.GetAsync<DashboardSummaryDto>(AnalyticConstants.DASHBOARD);
        if (summary != null)
        {
            ProductsCountTextBlock.Text = $"{summary.TotalProducts} / {summary.TotalRecipes}";
            ActiveBatchesTextBlock.Text = summary.ActiveBatchesCount.ToString();
            CustomersCountTextBlock.Text = summary.TotalCustomers.ToString();
            TotalStockTextBlock.Text = summary.TotalStockQuantity.ToString("F2");
            TotalRevenueTextBlock.Text = $"${summary.TotalSalesRevenue:F2}";
        }
    }
}