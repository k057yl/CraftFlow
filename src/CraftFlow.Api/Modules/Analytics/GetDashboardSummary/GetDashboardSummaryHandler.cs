using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Enums.Inventory;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Analytics.GetDashboardSummary;

public class GetDashboardSummaryHandler : IRequestHandler<GetDashboardSummaryQuery, Result<DashboardSummaryDto>>
{
    private readonly AppDbContext _dbContext;

    public GetDashboardSummaryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<DashboardSummaryDto>> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var fromDate = request.FromDate ?? DateTime.UtcNow.AddDays(-30);
        var toDate = request.ToDate ?? DateTime.UtcNow;

        var totalProducts = await _dbContext.Products.AsNoTracking().CountAsync(cancellationToken);
        var totalRecipes = await _dbContext.Recipes.AsNoTracking().CountAsync(cancellationToken);

        var activeBatches = await _dbContext.ProductionBatches
            .AsNoTracking()
            .CountAsync(b => b.State == BatchState.IN_PROGRESS, cancellationToken);

        var totalCustomers = await _dbContext.Customers.AsNoTracking().CountAsync(cancellationToken);

        var rawStock = await _dbContext.StockLots
            .AsNoTracking()
            .Where(s => s.ProductionBatchId == null)
            .SumAsync(s => (decimal?)s.Quantity, cancellationToken) ?? 0m;

        var finishedStock = await _dbContext.StockLots
            .AsNoTracking()
            .Where(s => s.ProductionBatchId != null)
            .SumAsync(s => (decimal?)s.Quantity, cancellationToken) ?? 0m;

        var totalRevenue = await _dbContext.SalesOrders
            .AsNoTracking()
            .Where(o => o.CreatedAt >= fromDate && o.CreatedAt <= toDate)
            .SumAsync(o => (decimal?)o.TotalAmount, cancellationToken) ?? 0m;

        var expirationThreshold = DateTime.UtcNow.AddDays(7);
        var expiringLotsCount = await _dbContext.StockLots
            .AsNoTracking()
            .CountAsync(s => s.ExpirationDate.HasValue
                          && s.ExpirationDate.Value <= expirationThreshold
                          && s.Quantity > 0, cancellationToken);

        var overdueSanitationCount = await _dbContext.StorageLocations
            .AsNoTracking()
            .CountAsync(l => l.NextWashDueDate.HasValue && l.NextWashDueDate.Value < DateTime.UtcNow, cancellationToken);

        var pendingProcurementsCount = await _dbContext.PurchaseOrders
            .AsNoTracking()
            .CountAsync(p => (int)p.Status != 3 && (int)p.Status != 4, cancellationToken);

        var summary = new DashboardSummaryDto(
            totalProducts,
            totalRecipes,
            activeBatches,
            totalCustomers,
            rawStock,
            finishedStock,
            totalRevenue,
            expiringLotsCount,
            overdueSanitationCount,
            pendingProcurementsCount
        );

        return Result.Success(summary);
    }
}