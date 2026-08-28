namespace CraftFlow.Wpf.Models;
public record DashboardSummaryDto(
    int TotalProducts,
    int TotalRecipes,
    int ActiveBatchesCount,
    int TotalCustomers,
    decimal TotalStockQuantity,
    decimal TotalSalesRevenue
);
