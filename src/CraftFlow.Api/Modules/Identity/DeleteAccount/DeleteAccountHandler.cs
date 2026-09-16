using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Identity.DeleteAccount;

public class DeleteAccountHandler : IRequestHandler<DeleteAccountCommand, Result<bool>>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public DeleteAccountHandler(AppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<Result<bool>> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var userId = _tenantContext.UserId;
        var tenantId = _tenantContext.TenantId;

        var user = await _dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null || !string.Equals(user.Email.Trim(), request.ConfirmationEmail?.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<bool>(Error.Validation("CONFIRMATION_EMAIL_MISMATCH"));
        }

        await HardDeleteTenantDataAsync(tenantId, _dbContext, cancellationToken);

        return Result.Success(true);
    }

    public static async Task HardDeleteTenantDataAsync(Guid tenantId, AppDbContext dbContext, CancellationToken cancellationToken)
    {
        var organization = await dbContext.Organizations.IgnoreQueryFilters().FirstOrDefaultAsync(o => o.Id == tenantId, cancellationToken);
        if (organization == null) return;

        var users = await dbContext.Users.IgnoreQueryFilters().Where(u => u.TenantId == tenantId).ToListAsync(cancellationToken);
        var accessKeys = await dbContext.TenantAccessKeys.IgnoreQueryFilters().Where(k => k.TenantId == tenantId).ToListAsync(cancellationToken);
        var subscriptions = await dbContext.TenantSubscriptions.IgnoreQueryFilters().Where(s => s.TenantId == tenantId).ToListAsync(cancellationToken);
        var payments = await dbContext.SubscriptionPayments.IgnoreQueryFilters().Where(p => p.TenantId == tenantId).ToListAsync(cancellationToken);
        var auditLogs = await dbContext.AuditLogs.IgnoreQueryFilters().Where(a => a.TenantId == tenantId).ToListAsync(cancellationToken);

        var warehouses = await dbContext.Warehouses.IgnoreQueryFilters().Where(w => w.TenantId == tenantId).ToListAsync(cancellationToken);
        var stockLots = await dbContext.StockLots.IgnoreQueryFilters().Where(s => s.TenantId == tenantId).ToListAsync(cancellationToken);
        var rawMaterials = await dbContext.RawMaterials.IgnoreQueryFilters().Where(r => r.TenantId == tenantId).ToListAsync(cancellationToken);
        var products = await dbContext.Products.IgnoreQueryFilters().Where(p => p.TenantId == tenantId).ToListAsync(cancellationToken);
        var recipes = await dbContext.Recipes.IgnoreQueryFilters().Where(r => r.TenantId == tenantId).ToListAsync(cancellationToken);
        var productionBatches = await dbContext.ProductionBatches.IgnoreQueryFilters().Where(b => b.TenantId == tenantId).ToListAsync(cancellationToken);
        var suppliers = await dbContext.Suppliers.IgnoreQueryFilters().Where(s => s.TenantId == tenantId).ToListAsync(cancellationToken);
        var purchaseOrders = await dbContext.PurchaseOrders.IgnoreQueryFilters().Where(p => p.TenantId == tenantId).ToListAsync(cancellationToken);
        var customers = await dbContext.Customers.IgnoreQueryFilters().Where(c => c.TenantId == tenantId).ToListAsync(cancellationToken);
        var salesOrders = await dbContext.SalesOrders.IgnoreQueryFilters().Where(s => s.TenantId == tenantId).ToListAsync(cancellationToken);
        var agingChambers = await dbContext.AgingChambers.IgnoreQueryFilters().Where(a => a.TenantId == tenantId).ToListAsync(cancellationToken);
        var agingLots = await dbContext.AgingLots.IgnoreQueryFilters().Where(a => a.TenantId == tenantId).ToListAsync(cancellationToken);

        dbContext.Users.RemoveRange(users);
        dbContext.TenantAccessKeys.RemoveRange(accessKeys);
        dbContext.SubscriptionPayments.RemoveRange(payments);
        dbContext.TenantSubscriptions.RemoveRange(subscriptions);
        dbContext.AuditLogs.RemoveRange(auditLogs);

        dbContext.StockLots.RemoveRange(stockLots);
        dbContext.Warehouses.RemoveRange(warehouses);
        dbContext.ProductionBatches.RemoveRange(productionBatches);
        dbContext.Recipes.RemoveRange(recipes);
        dbContext.Products.RemoveRange(products);
        dbContext.RawMaterials.RemoveRange(rawMaterials);
        dbContext.PurchaseOrders.RemoveRange(purchaseOrders);
        dbContext.Suppliers.RemoveRange(suppliers);
        dbContext.SalesOrders.RemoveRange(salesOrders);
        dbContext.Customers.RemoveRange(customers);
        dbContext.AgingLots.RemoveRange(agingLots);
        dbContext.AgingChambers.RemoveRange(agingChambers);

        dbContext.Organizations.Remove(organization);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}