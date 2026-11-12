using CraftFlow.Api.Core.MultiTenancy;
using CraftFlow.Api.Core.Persistence;
using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.ActivateFree;

public class ActivateFreeModuleHandler : IRequestHandler<ActivateFreeModuleCommand, Result<bool>>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public ActivateFreeModuleHandler(AppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<Result<bool>> Handle(ActivateFreeModuleCommand request, CancellationToken cancellationToken)
    {
        var code = request.PlanCode.Trim().ToUpperInvariant();
        if (!code.EndsWith("_FREE"))
        {
            return Result.Failure<bool>(Error.Validation("Доступно только подключение бесплатных (FREE) тарифов."));
        }

        var plan = await _dbContext.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Code == code, cancellationToken);

        if (plan == null)
        {
            return Result.Failure<bool>(Error.NotFound(ErrorCodes.General.NOT_FOUND));
        }

        var tenantId = _tenantContext.TenantId;
        var now = DateTime.UtcNow;

        var existingActive = await _dbContext.TenantSubscriptions
            .Include(s => s.Plan)
            .AnyAsync(s => s.TenantId == tenantId
                        && s.Plan.TargetModule == plan.TargetModule
                        && (!s.ExpiresAtUtc.HasValue || s.ExpiresAtUtc > now), cancellationToken);

        if (existingActive)
        {
            return Result.Failure<bool>(Error.Validation("Этот цех уже активирован."));
        }

        var subscription = TenantSubscription.Create(tenantId, plan.Id, expiresAtUtc: null);
        _dbContext.TenantSubscriptions.Add(subscription);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(true);
    }
}