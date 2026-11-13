using CraftFlow.Api.Core.MultiTenancy;
using CraftFlow.Api.Core.Persistence;
using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Enums.Subscription;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.RejectInvoice;

public class RejectInvoiceHandler : IRequestHandler<RejectInvoiceCommand, Result<bool>>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public RejectInvoiceHandler(AppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<Result<bool>> Handle(RejectInvoiceCommand request, CancellationToken cancellationToken)
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

        invoice.Reject();

        var paymentLog = SubscriptionPayment.Create(
            invoice.TenantId,
            amount: 0,
            planCode: invoice.PlanCode,
            daysAdded: 0,
            description: $"Отклонение заявки #{invoice.Id.ToString()[..8]} (оплата не поступила)"
        );
        _dbContext.SubscriptionPayments.Add(paymentLog);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(true);
    }
}