using CraftFlow.Api.Common.Audit;
using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Modules.Aging.Domain;
using CraftFlow.Api.Modules.Catalog.Domain;
using CraftFlow.Api.Modules.Identity.Domain;
using CraftFlow.Api.Modules.Inventory.Domain;
using CraftFlow.Api.Modules.Procurement.Domain;
using CraftFlow.Api.Modules.Production.Domain;
using CraftFlow.Api.Modules.Sales.Domain;
using CraftFlow.Api.Modules.Sanitation.Domain;
using CraftFlow.Api.Modules.Subscriptions.Domain;
using CraftFlow.SharedKernel.Domain;
using CraftFlow.SharedKernel.Enums.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

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
    public DbSet<OrganizationMember> OrganizationMembers => Set<OrganizationMember>();
    public DbSet<UnitOfMeasure> UnitsOfMeasure => Set<UnitOfMeasure>();
    public DbSet<RawMaterial> RawMaterials => Set<RawMaterial>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<StorageLocation> StorageLocations => Set<StorageLocation>();
    public DbSet<StockLot> StockLots => Set<StockLot>();
    public DbSet<ProductionBatch> ProductionBatches => Set<ProductionBatch>();
    public DbSet<ConsumedIngredient> ConsumedIngredients => Set<ConsumedIngredient>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<User> Users => Set<User>();
    public DbSet<SystemAdmin> SystemAdmins => Set<SystemAdmin>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
    public DbSet<AgingChamber> AgingChambers => Set<AgingChamber>();
    public DbSet<AgingLot> AgingLots => Set<AgingLot>();
    public DbSet<AgingLotItem> AgingLotItems => Set<AgingLotItem>();
    public DbSet<TenantSubscription> TenantSubscriptions => Set<TenantSubscription>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<TenantAccessKey> TenantAccessKeys => Set<TenantAccessKey>();
    public DbSet<SubscriptionPayment> SubscriptionPayments => Set<SubscriptionPayment>();
    public DbSet<SanitationRule> SanitationRules => Set<SanitationRule>();
    public DbSet<SanitationTask> SanitationTasks => Set<SanitationTask>();
    public DbSet<SanitationLog> SanitationLogs => Set<SanitationLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("public");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.Entity<AuditLog>().HasQueryFilter(e =>
            _tenantContext.Role == TenantRole.SUPER_ADMIN || e.TenantId == _tenantContext.TenantId);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.IsOwned() || entityType.ClrType == typeof(AuditLog))
            {
                continue;
            }

            var clrType = entityType.ClrType;
            var isTenant = typeof(ITenantEntity).IsAssignableFrom(clrType);

            if (isTenant)
            {
                var parameter = Expression.Parameter(clrType, "e");

                var roleProperty = Expression.Property(Expression.Constant(_tenantContext), nameof(ITenantContext.Role));
                var isSuperAdmin = Expression.Equal(roleProperty, Expression.Constant(TenantRole.SUPER_ADMIN));

                var tenantIdProperty = Expression.Property(parameter, nameof(ITenantEntity.TenantId));
                var contextTenantId = Expression.Property(Expression.Constant(_tenantContext), nameof(ITenantContext.TenantId));
                var isSameTenant = Expression.Equal(tenantIdProperty, contextTenantId);

                var filterBody = Expression.OrElse(isSuperAdmin, isSameTenant);
                var lambda = Expression.Lambda(filterBody, parameter);

                modelBuilder.Entity(clrType).HasQueryFilter(lambda);
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
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

        var auditEntries = new List<(EntityEntry Entry, string EntityName, string Action, Dictionary<string, object?> Changes)>();

        var entriesToAudit = ChangeTracker.Entries()
            .Where(e => e.Entity is not AuditLog &&
                       (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
            .ToList();

        foreach (var entry in entriesToAudit)
        {
            var entityName = entry.Entity.GetType().Name;
            var action = entry.State.ToString();
            var changes = new Dictionary<string, object?>();

            if (entry.State == EntityState.Added)
            {
                foreach (var prop in entry.CurrentValues.Properties)
                {
                    changes[prop.Name] = entry.CurrentValues[prop];
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                foreach (var prop in entry.Properties.Where(p => p.IsModified))
                {
                    changes[prop.Metadata.Name] = new
                    {
                        Old = prop.OriginalValue,
                        New = prop.CurrentValue
                    };
                }
            }
            else if (entry.State == EntityState.Deleted)
            {
                foreach (var prop in entry.OriginalValues.Properties)
                {
                    changes[prop.Name] = entry.OriginalValues[prop];
                }
            }

            auditEntries.Add((entry, entityName, action, changes));
        }

        var result = await base.SaveChangesAsync(cancellationToken);

        if (auditEntries.Count > 0)
        {
            var logs = new List<AuditLog>();
            foreach (var item in auditEntries)
            {
                var changesJson = JsonSerializer.Serialize(item.Changes);
                var details = $"{item.Action} {item.EntityName}";

                logs.Add(AuditLog.Create(
                    _tenantContext.TenantId,
                    _tenantContext.UserId,
                    _tenantContext.UserEmail ?? string.Empty,
                    item.EntityName,
                    item.Action,
                    details,
                    changesJson
                ));
            }

            AuditLogs.AddRange(logs);
            await base.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    private void SetTenantAndActiveFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : Entity, ITenantEntity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e =>
            (_tenantContext.Role == TenantRole.SUPER_ADMIN || e.TenantId == _tenantContext.TenantId));
    }

    private void SetTenantFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantEntity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e =>
            _tenantContext.Role == TenantRole.SUPER_ADMIN || e.TenantId == _tenantContext.TenantId);
    }

    private void SetActiveFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : Entity
    {
    }

    private sealed class DesignTimeTenantContext : ITenantContext
    {
        public Guid TenantId => Guid.Empty;
        public Guid UserId => Guid.Empty;
        public string? UserEmail => null;
        public TenantRole Role => TenantRole.SUPER_ADMIN;
        public bool IsResolved => true;
    }
}