using CraftFlow.Api.Core.MultiTenancy;
using CraftFlow.Api.Core.Persistence;
using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Enums.Subscription;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.ApproveInvoice;

public class ApproveInvoiceHandler : IRequestHandler<ApproveInvoiceCommand, Result<bool>>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public ApproveInvoiceHandler(AppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<Result<bool>> Handle(ApproveInvoiceCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.IsSuperAdmin)
        {
            return Result.Failure<bool>(Error.Validation(ErrorCodes.Auth.ACCESS_DENIED));
        }

        var invoice = await _dbContext.SubscriptionInvoices
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken);

        if (invoice == null || invoice.Status != InvoiceStatus.PENDING)
        {
            return Result.Failure<bool>(Error.NotFound("INVOICE_NOT_FOUND_OR_PROCESSED"));
        }

        var targetPlan = await _dbContext.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Code == invoice.PlanCode, cancellationToken);

        if (targetPlan == null)
        {
            return Result.Failure<bool>(Error.NotFound("PLAN_NOT_FOUND"));
        }

        var subscription = await _dbContext.TenantSubscriptions
            .IgnoreQueryFilters()
            .Include(s => s.Plan)
            .Where(s => s.TenantId == invoice.TenantId && s.Plan.TargetModule == targetPlan.TargetModule)
            .FirstOrDefaultAsync(cancellationToken);

        var now = DateTime.UtcNow;

        if (subscription == null)
        {
            subscription = TenantSubscription.Create(invoice.TenantId, targetPlan.Id);
            if (invoice.DaysRequested > 0)
            {
                subscription.Activate(now.AddDays(invoice.DaysRequested));
            }
            _dbContext.TenantSubscriptions.Add(subscription);
        }
        else
        {
            var baseDate = subscription.ExpiresAtUtc.HasValue && subscription.ExpiresAtUtc.Value > now
                ? subscription.ExpiresAtUtc.Value
                : now;

            subscription.Extend(baseDate.AddDays(invoice.DaysRequested));
            subscription.ChangePlan(targetPlan.Id);
        }

        var payment = SubscriptionPayment.Create(
            invoice.TenantId,
            amount: invoice.Amount,
            planCode: targetPlan.Code,
            daysAdded: invoice.DaysRequested,
            description: $"Подтверждение заявки #{invoice.Id.ToString()[..8]} ({targetPlan.Name})"
        );
        _dbContext.SubscriptionPayments.Add(payment);

        invoice.Approve();

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(true);
    }
}