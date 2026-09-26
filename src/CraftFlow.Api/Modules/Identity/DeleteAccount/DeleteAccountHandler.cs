using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Identity.Domain;
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
        if (_tenantContext.Role != TenantRole.Owner && !_tenantContext.IsSuperAdmin)
        {
            return Result.Failure<bool>(Error.Validation("ONLY_OWNER_CAN_DELETE_ACCOUNT"));
        }

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
        await dbContext.AuditLogs.IgnoreQueryFilters().Where(a => a.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);
        await dbContext.SubscriptionPayments.IgnoreQueryFilters().Where(p => p.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);
        await dbContext.TenantSubscriptions.IgnoreQueryFilters().Where(s => s.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);
        await dbContext.TenantAccessKeys.IgnoreQueryFilters().Where(k => k.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);

        await dbContext.StockLots.IgnoreQueryFilters().Where(s => s.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);
        await dbContext.AgingLotItems.IgnoreQueryFilters()
            .Where(i => dbContext.AgingLots.IgnoreQueryFilters().Any(l => l.Id == i.AgingLotId && l.TenantId == tenantId))
            .ExecuteDeleteAsync(cancellationToken);
        await dbContext.AgingLots.IgnoreQueryFilters().Where(a => a.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);
        await dbContext.AgingChambers.IgnoreQueryFilters().Where(a => a.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);

        await dbContext.ProductionBatches.IgnoreQueryFilters().Where(b => b.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);
        await dbContext.PurchaseOrderItems.IgnoreQueryFilters()
            .Where(i => dbContext.PurchaseOrders.IgnoreQueryFilters().Any(o => o.Id == i.PurchaseOrderId && o.TenantId == tenantId))
            .ExecuteDeleteAsync(cancellationToken);
        await dbContext.PurchaseOrders.IgnoreQueryFilters().Where(p => p.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);
        await dbContext.SalesOrders.IgnoreQueryFilters().Where(s => s.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);

        await dbContext.Recipes.IgnoreQueryFilters().Where(r => r.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);
        await dbContext.Products.IgnoreQueryFilters().Where(p => p.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);
        await dbContext.RawMaterials.IgnoreQueryFilters().Where(r => r.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);
        await dbContext.Warehouses.IgnoreQueryFilters().Where(w => w.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);
        await dbContext.Suppliers.IgnoreQueryFilters().Where(s => s.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);
        await dbContext.Customers.IgnoreQueryFilters().Where(c => c.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);

        await dbContext.Users.IgnoreQueryFilters().Where(u => u.TenantId == tenantId).ExecuteDeleteAsync(cancellationToken);
        await dbContext.Organizations.IgnoreQueryFilters().Where(o => o.Id == tenantId).ExecuteDeleteAsync(cancellationToken);
    }
}