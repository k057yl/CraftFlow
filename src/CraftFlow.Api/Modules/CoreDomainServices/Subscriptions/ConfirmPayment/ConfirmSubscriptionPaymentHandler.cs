using CraftFlow.Api.Core.MultiTenancy;
using CraftFlow.Api.Core.Persistence;
using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.ConfirmPayment;

public class ConfirmSubscriptionPaymentHandler : IRequestHandler<ConfirmSubscriptionPaymentCommand, Result>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public ConfirmSubscriptionPaymentHandler(AppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(ConfirmSubscriptionPaymentCommand request, CancellationToken cancellationToken)
    {
        var targetTenantId = _tenantContext.IsSuperAdmin
            ? request.TargetTenantId ?? _tenantContext.TenantId
            : _tenantContext.TenantId;

        if (targetTenantId == Guid.Empty)
        {
            return Result.Failure(Error.Validation(ErrorCodes.General.INVALID_TENANT));
        }

        var plan = await _dbContext.Set<SubscriptionPlan>()
            .FirstOrDefaultAsync(p => p.Code == request.PlanCode, cancellationToken);

        if (plan == null)
        {
            return Result.Failure(Error.NotFound(ErrorCodes.Saas.PLAN_NOT_FOUND));
        }

        var subscription = await _dbContext.Set<TenantSubscription>()
            .FirstOrDefaultAsync(s => s.TenantId == targetTenantId && s.PlanId == plan.Id, cancellationToken);

        var nowUtc = DateTime.UtcNow;

        if (subscription == null)
        {
            subscription = TenantSubscription.Create(targetTenantId, plan.Id);
            _dbContext.Set<TenantSubscription>().Add(subscription);
        }

        var baseDate = subscription.ExpiresAtUtc.HasValue && subscription.ExpiresAtUtc.Value > nowUtc
            ? subscription.ExpiresAtUtc.Value
            : nowUtc;

        var newExpirationUtc = baseDate.AddDays(request.Days);
        subscription.Activate(newExpirationUtc);

        var payment = SubscriptionPayment.Create(
            targetTenantId,
            amount: 990m,
            planCode: plan.Code,
            daysAdded: request.Days,
            description: $"Активация тарифа {plan.Name}"
        );

        _dbContext.Set<SubscriptionPayment>().Add(payment);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}