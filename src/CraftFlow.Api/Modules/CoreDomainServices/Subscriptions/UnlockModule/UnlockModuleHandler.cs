using CraftFlow.Api.Core.MultiTenancy;
using CraftFlow.Api.Core.Persistence;
using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.Domain;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.UnlockModule;

public class UnlockModuleHandler : IRequestHandler<UnlockModuleCommand, Result<bool>>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public UnlockModuleHandler(AppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<Result<bool>> Handle(UnlockModuleCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        var targetFreePlan = await _dbContext.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Code == request.FreePlanCode.ToUpperInvariant(), cancellationToken);

        if (targetFreePlan == null || !targetFreePlan.Code.EndsWith("_FREE"))
        {
            return Result.Failure<bool>(Error.Validation("INVALID_MODULE_PLAN"));
        }

        var tenantSubscriptions = await _dbContext.TenantSubscriptions
            .Include(s => s.Plan)
            .Where(s => s.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        if (tenantSubscriptions.Any(s => s.Plan.TargetModule == targetFreePlan.TargetModule))
        {
            return Result.Failure<bool>(Error.Validation("MODULE_ALREADY_UNLOCKED"));
        }

        var hasPaidActiveSubscription = tenantSubscriptions.Any(s =>
            !s.Plan.Code.EndsWith("_FREE") &&
            (s.State == SubscriptionState.Active || s.State == SubscriptionState.Trial));

        if (!hasPaidActiveSubscription)
        {
            return Result.Failure<bool>(Error.Validation("REQUIRES_PAID_SUBSCRIPTION_TO_UNLOCK_ADDONS"));
        }

        var newSubscription = TenantSubscription.CreateFree(tenantId, targetFreePlan.Id);
        _dbContext.TenantSubscriptions.Add(newSubscription);

        var payment = SubscriptionPayment.Create(
            tenantId,
            amount: 10m,
            planCode: targetFreePlan.Code,
            daysAdded: 0,
            description: $"Единоразовая разблокировка модуля: {targetFreePlan.Name}"
        );
        _dbContext.SubscriptionPayments.Add(payment);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}