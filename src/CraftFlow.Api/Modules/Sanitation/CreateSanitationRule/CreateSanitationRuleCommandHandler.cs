using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Sanitation.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Enums.Sanitation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Sanitation.CreateSanitationRule;

public sealed class CreateSanitationRuleCommandHandler : IRequestHandler<CreateSanitationRuleCommand, Guid>
{
    private readonly AppDbContext _context;
    private readonly ITenantContext _tenantContext;

    public CreateSanitationRuleCommandHandler(AppDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<Guid> Handle(CreateSanitationRuleCommand request, CancellationToken cancellationToken)
    {
        var targetExists = request.TargetType switch
        {
            SanitationTargetType.WAREHOUSE => await _context.Warehouses.AnyAsync(w => w.Id == request.TargetId, cancellationToken),
            SanitationTargetType.AGING_CHAMBER => await _context.AgingChambers.AnyAsync(c => c.Id == request.TargetId, cancellationToken),
            SanitationTargetType.STORAGE_LOCATION => await _context.StorageLocations.AnyAsync(l => l.Id == request.TargetId, cancellationToken),
            _ => false
        };

        if (!targetExists)
            throw new KeyNotFoundException(ErrorCodes.General.NOT_FOUND);

        var existingRule = await _context.SanitationRules
            .FirstOrDefaultAsync(r => r.TargetType == request.TargetType && r.TargetId == request.TargetId, cancellationToken);

        if (existingRule != null)
        {
            _context.SanitationRules.Remove(existingRule);
        }

        var rule = SanitationRule.Create(
            _tenantContext.TenantId,
            request.TargetType,
            request.TargetId,
            request.TriggerType,
            request.RequiredChemicals,
            intervalDays: request.IntervalDays,
            maxBatchCount: request.MaxBatchCount,
            nextScheduledAt: request.NextScheduledAt
        );

        _context.SanitationRules.Add(rule);
        await _context.SaveChangesAsync(cancellationToken);

        return rule.Id;
    }
}