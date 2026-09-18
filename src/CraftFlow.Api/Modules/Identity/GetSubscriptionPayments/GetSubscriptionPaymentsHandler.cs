using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Subscriptions.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Identity.GetSubscriptionPayments;

public class GetSubscriptionPaymentsHandler : IRequestHandler<GetSubscriptionPaymentsQuery, Result<List<PaymentDto>>>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public GetSubscriptionPaymentsHandler(AppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<Result<List<PaymentDto>>> Handle(GetSubscriptionPaymentsQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.IsSuperAdmin)
        {
            return Result.Failure<List<PaymentDto>>(Error.Validation(ErrorCodes.Auth.INVALID_CREDENTIALS));
        }

        var payments = await _dbContext.Set<SubscriptionPayment>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(p => p.TenantId == request.OrganizationId)
            .OrderByDescending(p => p.CreatedAtUtc)
            .Select(p => new PaymentDto(p.Id, p.CreatedAtUtc, p.PlanCode, p.DaysAdded, p.Description))
            .ToListAsync(cancellationToken);

        return Result.Success(payments);
    }
}