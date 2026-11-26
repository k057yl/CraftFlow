using CraftFlow.SharedKernel.Enums;
using CraftFlow.SharedKernel.Enums.Subscription;

namespace CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.Services;

public interface ITenantEntitlementService
{
    Task<bool> HasOrganizationFeatureAsync(PlanFeatures feature, CancellationToken ct = default);
    Task<bool> HasModuleFeatureAsync(AppModule module, PlanFeatures feature, CancellationToken ct = default);
    Task<int> GetAuditRetentionDaysAsync(CancellationToken ct = default);
    Task<int> GetMaxMonthlyBatchesAsync(AppModule module, CancellationToken ct = default);
    Task<int> GetMaxChambersAsync(AppModule module, CancellationToken ct = default);
    Task<int> GetMaxWarehousesAsync(CancellationToken ct = default);
    Task<int> GetMaxUsersAsync(CancellationToken ct = default);
    void InvalidateCache(Guid tenantId);
}