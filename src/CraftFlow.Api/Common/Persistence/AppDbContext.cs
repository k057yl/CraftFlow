using System.Reflection;
using CraftFlow.Api.Common.Audit;
using CraftFlow.Api.Common.Constants;
using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Modules.Aging.Domain;
using CraftFlow.Api.Modules.Catalog.Domain;
using CraftFlow.Api.Modules.Identity.Domain;
using CraftFlow.Api.Modules.Inventory.Domain;
using CraftFlow.Api.Modules.Procurement.Domain;
using CraftFlow.Api.Modules.Production.Domain;
using CraftFlow.Api.Modules.Sales.Domain;
using CraftFlow.Api.Modules.Subscriptions.Domain;
using CraftFlow.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Common.Persistence;

public class AppDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
        _tenantContext = new DesignTimeTenantContext();
    }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<UnitOfMeasure> UnitsOfMeasure => Set<UnitOfMeasure>();
    public DbSet<RawMaterial> RawMaterials => Set<RawMaterial>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<StockLot> StockLots => Set<StockLot>();
    public DbSet<ProductionBatch> ProductionBatches => Set<ProductionBatch>();
    public DbSet<ConsumedIngredient> ConsumedIngredients => Set<ConsumedIngredient>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<User> Users => Set<User>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
    public DbSet<AgingChamber> AgingChambers => Set<AgingChamber>();
    public DbSet<AgingLot> AgingLots => Set<AgingLot>();
    public DbSet<TenantSubscription> TenantSubscriptions => Set<TenantSubscription>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<TenantAccessKey> TenantAccessKeys => Set<TenantAccessKey>();
    public DbSet<SubscriptionPayment> SubscriptionPayments => Set<SubscriptionPayment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("public");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            if (clrType == typeof(User))
            {
                SetUserFilter(modelBuilder);
                continue;
            }

            var isTenant = typeof(ITenantEntity).IsAssignableFrom(clrType);
            var isEntity = typeof(Entity).IsAssignableFrom(clrType);

            if (isTenant && isEntity)
            {
                var method = typeof(AppDbContext)
                    .GetMethod(nameof(SetTenantAndActiveFilter), BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.MakeGenericMethod(clrType);

                method?.Invoke(this, new object[] { modelBuilder });
            }
            else if (isTenant)
            {
                var method = typeof(AppDbContext)
                    .GetMethod(nameof(SetTenantFilter), BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.MakeGenericMethod(clrType);

                method?.Invoke(this, new object[] { modelBuilder });
            }
            else if (isEntity)
            {
                var method = typeof(AppDbContext)
                    .GetMethod(nameof(SetActiveFilter), BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.MakeGenericMethod(clrType);

                method?.Invoke(this, new object[] { modelBuilder });
            }
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                var currentTenantId = entry.Property(nameof(ITenantEntity.TenantId)).CurrentValue as Guid?;

                if (!currentTenantId.HasValue || currentTenantId.Value == Guid.Empty)
                {
                    if (_tenantContext.TenantId != Guid.Empty)
                    {
                        entry.Property(nameof(ITenantEntity.TenantId)).CurrentValue = _tenantContext.TenantId;
                    }
                }
            }
        }

        var auditEntries = ChangeTracker.Entries()
            .Where(e => e.Entity is not AuditLog && (e.State == EntityState.Added || e.State == EntityState.Modified))
            .Select(e => AuditLog.Create(
                _tenantContext.TenantId,
                _tenantContext.UserId,
                e.Entity.GetType().Name,
                e.State.ToString(),
                string.Format(CoreConstants.Audit.AUDIT_CHANGE_FORMAT, e.Entity.GetType().Name)
            ))
            .ToList();

        if (auditEntries.Count > 0)
        {
            AuditLogs.AddRange(auditEntries);
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    private void SetUserFilter(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasQueryFilter(u =>
            (_tenantContext.IsSuperAdmin || (u.TenantId == _tenantContext.TenantId && u.Role != TenantRole.SuperAdmin)));
    }

    private void SetTenantAndActiveFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : Entity, ITenantEntity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e =>
            (_tenantContext.IsSuperAdmin || e.TenantId == _tenantContext.TenantId) && e.IsActive);
    }

    private void SetTenantFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantEntity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e =>
            _tenantContext.IsSuperAdmin || e.TenantId == _tenantContext.TenantId);
    }

    private void SetActiveFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : Entity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => e.IsActive);
    }
}

public class DesignTimeTenantContext : ITenantContext
{
    public Guid TenantId => Guid.Empty;
    public Guid UserId => Guid.Empty;
    public TenantRole Role => TenantRole.SuperAdmin;
    public bool IsResolved => true;
}