using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Production.Domain;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Analytics.GetDashboardSummary
{
    public class GetDashboardSummaryHandler : IRequestHandler<GetDashboardSummaryQuery, Result<DashboardSummaryDto>>
    {
        private readonly AppDbContext _dbContext;

        public GetDashboardSummaryHandler(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<DashboardSummaryDto>> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
        {
            var totalProducts = await _dbContext.Products.CountAsync(cancellationToken);
            var totalRecipes = await _dbContext.Recipes.CountAsync(cancellationToken);
            var activeBatches = await _dbContext.ProductionBatches
                .CountAsync(b => b.Status == BatchStatus.InProgress, cancellationToken);
            var totalCustomers = await _dbContext.Customers.CountAsync(cancellationToken);

            var totalStock = await _dbContext.StockLots
                .SumAsync(s => (decimal?)s.Quantity, cancellationToken) ?? 0m;

            var totalRevenue = await _dbContext.SalesOrders
                .SumAsync(o => (decimal?)o.TotalAmount, cancellationToken) ?? 0m;

            var summary = new DashboardSummaryDto(
                totalProducts,
                totalRecipes,
                activeBatches,
                totalCustomers,
                totalStock,
                totalRevenue
            );

            return Result.Success(summary);
        }
    }
}
