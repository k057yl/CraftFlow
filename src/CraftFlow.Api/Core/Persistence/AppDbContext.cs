using CraftFlow.Api.Core.Domain;
using CraftFlow.Api.Core.MultiTenancy;
using CraftFlow.Api.Modules.CoreDomainServices.Catalog.Domain;
using CraftFlow.Api.Modules.CoreDomainServices.Identity.Domain;
using CraftFlow.Api.Modules.CoreDomainServices.Inventory.Domain;
using CraftFlow.Api.Modules.CoreDomainServices.Procurement.Domain;
using CraftFlow.Api.Modules.CoreDomainServices.Production.Domain;
using CraftFlow.Api.Modules.CoreDomainServices.Sales.Domain;
using CraftFlow.Api.Modules.CoreDomainServices.Sanitation.Domain;
using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.Domain;
using CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Aging.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Domain;
using CraftFlow.SharedKernel.Enums.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;
using System.Text.Json;

namespace CraftFlow.Api.Core.Persistence;

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
    public DbSet<SubscriptionPayment> SubscriptionPayments => Set<SubscriptionPayment>();
    public DbSet<SubscriptionInvoice> SubscriptionInvoices => Set<SubscriptionInvoice>();
    public DbSet<SanitationRule> SanitationRules => Set<SanitationRule>();
    public DbSet<SanitationTask> SanitationTasks => Set<SanitationTask>();
    public DbSet<SanitationLog> SanitationLogs => Set<SanitationLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("public");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        ConfigureTenantGlobalFilters(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ProcessTenantEntities();

        var auditEntries = PrepareAuditEntries();

        var result = await base.SaveChangesAsync(cancellationToken);

        if (auditEntries.Count > 0)
        {
            await SaveAuditLogsAsync(auditEntries, cancellationToken);
        }

        return result;
    }

    #region Tenant & Filter Logic

    private void ConfigureTenantGlobalFilters(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>().HasQueryFilter(e =>
            _tenantContext.Role == TenantRole.SUPER_ADMIN || e.TenantId == _tenantContext.TenantId);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.IsOwned() || entityType.ClrType == typeof(AuditLog))
            {
                continue;
            }

            var clrType = entityType.ClrType;
            if (typeof(ITenantEntity).IsAssignableFrom(clrType))
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

    private void ProcessTenantEntities()
    {
        var currentTenantId = _tenantContext.TenantId;
        var isSuperAdmin = _tenantContext.Role == TenantRole.SUPER_ADMIN;

        foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    ValidateAndSetTenantIdOnAdd(entry, currentTenantId, isSuperAdmin);
                    break;

                case EntityState.Modified:
                    ValidateTenantIdOnModify(entry, currentTenantId, isSuperAdmin);
                    break;

                case EntityState.Deleted:
                    ValidateTenantIdOnDelete(entry, currentTenantId, isSuperAdmin);
                    break;
            }
        }
    }

    private static void ValidateAndSetTenantIdOnAdd(EntityEntry<ITenantEntity> entry, Guid currentTenantId, bool isSuperAdmin)
    {
        var entityTenantId = entry.Property(nameof(ITenantEntity.TenantId)).CurrentValue as Guid?;
        var hasExplicitTenantId = entityTenantId.HasValue && entityTenantId.Value != Guid.Empty;

        if (isSuperAdmin)
        {
            if (hasExplicitTenantId) return;
            if (currentTenantId != Guid.Empty)
            {
                entry.Property(nameof(ITenantEntity.TenantId)).CurrentValue = currentTenantId;
                return;
            }
            throw new InvalidOperationException(ErrorCodes.General.INVALID_TENANT);
        }

        if (hasExplicitTenantId)
        {
            if (currentTenantId != Guid.Empty && entityTenantId!.Value != currentTenantId)
            {
                throw new UnauthorizedAccessException(ErrorCodes.Auth.ACCESS_DENIED);
            }
            return;
        }

        if (currentTenantId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(ErrorCodes.General.INVALID_TENANT);
        }

        entry.Property(nameof(ITenantEntity.TenantId)).CurrentValue = currentTenantId;
    }

    private static void ValidateTenantIdOnModify(EntityEntry<ITenantEntity> entry, Guid currentTenantId, bool isSuperAdmin)
    {
        if (entry.Property(nameof(ITenantEntity.TenantId)).IsModified)
        {
            throw new UnauthorizedAccessException(ErrorCodes.Auth.ACCESS_DENIED);
        }

        if (!isSuperAdmin && currentTenantId != Guid.Empty && entry.Entity.TenantId != currentTenantId)
        {
            throw new UnauthorizedAccessException(ErrorCodes.Auth.ACCESS_DENIED);
        }
    }

    private static void ValidateTenantIdOnDelete(EntityEntry<ITenantEntity> entry, Guid currentTenantId, bool isSuperAdmin)
    {
        if (!isSuperAdmin && currentTenantId != Guid.Empty && entry.Entity.TenantId != currentTenantId)
        {
            throw new UnauthorizedAccessException(ErrorCodes.Auth.ACCESS_DENIED);
        }
    }

    #endregion

    #region Audit Logs Logic

    private List<(EntityEntry Entry, string EntityName, string Action, Dictionary<string, object?> Changes)> PrepareAuditEntries()
    {
        var auditEntries = new List<(EntityEntry, string, string, Dictionary<string, object?>)>();

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

        return auditEntries;
    }

    private async Task SaveAuditLogsAsync(
        List<(EntityEntry Entry, string EntityName, string Action, Dictionary<string, object?> Changes)> auditEntries,
        CancellationToken cancellationToken)
    {
        var logs = new List<AuditLog>();

        foreach (var item in auditEntries)
        {
            var changesJson = JsonSerializer.Serialize(item.Changes);
            var details = $"{item.Action} {item.EntityName}";

            var tenantIdForAudit = _tenantContext.TenantId != Guid.Empty
                ? _tenantContext.TenantId
                : (item.Entry.Entity is ITenantEntity te ? te.TenantId : Guid.Empty);

            logs.Add(AuditLog.Create(
                tenantIdForAudit,
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

    #endregion

    private sealed class DesignTimeTenantContext : ITenantContext
    {
        public Guid TenantId => Guid.Empty;
        public Guid UserId => Guid.Empty;
        public string? UserEmail => null;
        public TenantRole Role => TenantRole.SUPER_ADMIN;
        public bool IsResolved => true;
    }
}