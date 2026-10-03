using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Subscriptions.Domain;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Subscriptions.ConfirmPayment;

public class ConfirmSubscriptionPaymentHandler : IRequestHandler<ConfirmSubscriptionPaymentCommand, Result>
{
    private const string MSG_PLAN_NOT_FOUND = "PLAN_NOT_FOUND";

    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public ConfirmSubscriptionPaymentHandler(AppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(ConfirmSubscriptionPaymentCommand request, CancellationToken ct)
    {
        var tenantId = _tenantContext.TenantId;

        var plan = await _dbContext.Set<SubscriptionPlan>()
            .FirstOrDefaultAsync(p => p.Code == request.PlanCode, ct);

        if (plan == null)
        {
            return Result.Failure(Error.NotFound(MSG_PLAN_NOT_FOUND));
        }

        var subscription = await _dbContext.Set<TenantSubscription>()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId, ct);

        var newExpirationUtc = DateTime.UtcNow.AddDays(request.Days);

        if (subscription == null)
        {
            subscription = TenantSubscription.CreateFree(tenantId, plan.Id);
            subscription.Activate(newExpirationUtc);
            _dbContext.Set<TenantSubscription>().Add(subscription);
        }
        else
        {
            subscription.ChangePlan(plan.Id);
            subscription.Extend(newExpirationUtc);
        }

        var payment = SubscriptionPayment.Create(
            tenantId,
            amount: request.PlanCode == "FREE" ? 0m : 990m,
            planCode: plan.Code,
            daysAdded: request.Days,
            description: $"Тестовая активация тарифа {plan.Name}"
        );

        _dbContext.Set<SubscriptionPayment>().Add(payment);

        await _dbContext.SaveChangesAsync(ct);

        return Result.Success();
    }
}