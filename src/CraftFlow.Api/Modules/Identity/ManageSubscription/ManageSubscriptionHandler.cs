using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Subscriptions.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Identity.ManageSubscription;

public class ManageSubscriptionHandler : IRequestHandler<ManageSubscriptionCommand, Result<bool>>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public ManageSubscriptionHandler(AppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<Result<bool>> Handle(ManageSubscriptionCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.IsSuperAdmin)
        {
            return Result.Failure<bool>(Error.Validation(ErrorCodes.Auth.INVALID_CREDENTIALS));
        }

        var organization = await _dbContext.Organizations
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(o => o.Id == request.OrganizationId, cancellationToken);

        if (organization == null)
        {
            return Result.Failure<bool>(Error.NotFound(ErrorCodes.Auth.USER_NOT_FOUND));
        }

        if (request.IsActive) organization.Activate();
        else organization.Deactivate();

        var targetPlan = await _dbContext.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Code == request.PlanCode.ToUpperInvariant(), cancellationToken);

        if (targetPlan == null)
        {
            return Result.Failure<bool>(Error.NotFound("PLAN_NOT_FOUND"));
        }

        var subscription = await _dbContext.TenantSubscriptions
            .IgnoreQueryFilters()
            .Where(s => s.TenantId == request.OrganizationId)
            .OrderByDescending(s => s.ExpiresAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        var now = DateTime.UtcNow;

        if (subscription == null)
        {
            subscription = TenantSubscription.CreateFree(organization.Id, targetPlan.Id);
            if (request.AddDays > 0)
            {
                subscription.Activate(now.AddDays(request.AddDays));
            }
            _dbContext.TenantSubscriptions.Add(subscription);
        }
        else
        {
            var baseDate = subscription.ExpiresAtUtc.HasValue ? subscription.ExpiresAtUtc.Value : now;
            var newExpires = baseDate.AddDays(request.AddDays);
            if (newExpires < now) newExpires = now;

            subscription.Extend(newExpires);
            subscription.ChangePlan(targetPlan.Id);
        }

        if (request.AddDays != 0)
        {
            var actionText = request.AddDays > 0
                ? $"Продление подписки ({targetPlan.Code})"
                : $"Корректировка/Списание дней ({targetPlan.Code})";

            var paymentRecord = SubscriptionPayment.Create(
                organization.Id,
                amount: 0,
                planCode: targetPlan.Code,
                daysAdded: request.AddDays,
                description: actionText
            );

            _dbContext.Set<SubscriptionPayment>().Add(paymentRecord);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(true);
    }
}