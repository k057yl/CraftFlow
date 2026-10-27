namespace CraftFlow.Api.Modules.Analytics.GetDashboardSummary;

public record DashboardSummaryDto(
    int TotalProducts,
    int TotalRecipes,
    int ActiveBatchesCount,
    int TotalCustomers,
    decimal RawMaterialStockQuantity,
    decimal FinishedProductStockQuantity,
    decimal TotalSalesRevenue,
    int ExpiringStockLotsCount,
    int OverdueSanitationTasksCount,
    int PendingProcurementsCount
);