using CraftFlow.Api.Core.MultiTenancy;
using CraftFlow.Api.Core.Persistence;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Dtos.Subscription;
using CraftFlow.SharedKernel.Enums.Subscription;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.GetPendingInvoices;

public class GetPendingInvoicesHandler : IRequestHandler<GetPendingInvoicesQuery, Result<List<PendingInvoiceDto>>>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public GetPendingInvoicesHandler(AppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<Result<List<PendingInvoiceDto>>> Handle(GetPendingInvoicesQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.IsSuperAdmin)
        {
            return Result.Failure<List<PendingInvoiceDto>>(Error.Validation(ErrorCodes.Auth.ACCESS_DENIED));
        }

        var pendingInvoices = await _dbContext.SubscriptionInvoices
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(i => i.Status == InvoiceStatus.PENDING)
            .OrderByDescending(i => i.CreatedAtUtc)
            .Join(
                _dbContext.Organizations.IgnoreQueryFilters(),
                invoice => invoice.TenantId,
                org => org.Id,
                (invoice, org) => new PendingInvoiceDto(
                    invoice.Id,
                    invoice.TenantId,
                    org.Name,
                    invoice.PlanCode,
                    invoice.DaysRequested,
                    invoice.CreatedAtUtc
                )
            )
            .ToListAsync(cancellationToken);

        return Result.Success(pendingInvoices);
    }
}