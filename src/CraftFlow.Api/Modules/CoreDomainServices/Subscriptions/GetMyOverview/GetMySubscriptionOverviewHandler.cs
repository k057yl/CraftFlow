using CraftFlow.Api.Core.MultiTenancy;
using CraftFlow.Api.Core.Persistence;
using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.Services;
using CraftFlow.SharedKernel.Dtos.Subscription;
using CraftFlow.SharedKernel.Enums;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.GetMyOverview;

public class GetMySubscriptionOverviewHandler : IRequestHandler<GetMySubscriptionOverviewQuery, Result<MySubscriptionOverviewDto>>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly SubscriptionLifecycleService _lifecycleService;

    public GetMySubscriptionOverviewHandler(
        AppDbContext dbContext,
        ITenantContext tenantContext,
        SubscriptionLifecycleService lifecycleService)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _lifecycleService = lifecycleService;
    }

    public async Task<Result<MySubscriptionOverviewDto>> Handle(GetMySubscriptionOverviewQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        await _lifecycleService.DowngradeExpiredModulesAsync(tenantId, cancellationToken);

        var now = DateTime.UtcNow;

        var subscriptions = await _dbContext.TenantSubscriptions
            .AsNoTracking()
            .Include(s => s.Plan)
            .Where(s => s.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        var activeSubs = subscriptions
            .Where(s => !s.ExpiresAtUtc.HasValue || s.ExpiresAtUtc.Value > now)
            .ToList();

        var activeModules = AppModule.NONE;
        foreach (var sub in activeSubs)
        {
            activeModules |= sub.Plan.TargetModule;
        }

        var payments = await _dbContext.SubscriptionPayments
            .AsNoTracking()
            .Where(p => p.TenantId == tenantId)
            .OrderByDescending(p => p.CreatedAtUtc)
            .Select(p => new SubscriptionPaymentDto(
                p.Id,
                p.PlanCode,
                p.DaysAdded,
                p.Description,
                p.CreatedAtUtc
            ))
            .ToListAsync(cancellationToken);

        string badge = activeSubs.Any(s => s.ExpiresAtUtc.HasValue) ? "АКТИВНА" : "БАЗОВЫЙ (FREE)";
        string plansSummary = activeSubs.Count > 0
            ? string.Join(", ", activeSubs.Select(s => s.Plan.Name))
            : "Не подключены";

        DateTime? maxExpiration = activeSubs
            .Where(s => s.ExpiresAtUtc.HasValue)
            .Select(s => s.ExpiresAtUtc)
            .OrderByDescending(d => d)
            .FirstOrDefault();

        return Result.Success(new MySubscriptionOverviewDto(
            badge,
            plansSummary,
            maxExpiration,
            activeModules,
            payments
        ));
    }
}