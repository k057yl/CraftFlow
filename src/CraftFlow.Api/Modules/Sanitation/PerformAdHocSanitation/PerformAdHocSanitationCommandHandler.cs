using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Sanitation.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Enums.Sanitation;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Sanitation.PerformAdHocSanitation;

public sealed class PerformAdHocSanitationCommandHandler : IRequestHandler<PerformAdHocSanitationCommand, Result<Guid>>
{
    private readonly AppDbContext _context;
    private readonly ITenantContext _tenantContext;

    public PerformAdHocSanitationCommandHandler(AppDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(PerformAdHocSanitationCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.IsResolved || _tenantContext.TenantId == Guid.Empty)
            return Result.Failure<Guid>(ErrorCodes.Auth.UNAUTHORIZED);

        var userId = _tenantContext.UserId;
        var now = DateTime.UtcNow;
        var task = SanitationTask.Create(_tenantContext.TenantId, request.TargetType, request.TargetId, now);

        task.Start(userId, now);
        task.Complete(now, request.ChemicalsUsed, request.Notes);
        var log = task.Verify(userId, now, isPassed: true, verificationNotes: "Внеплановая экспресс-мойка");

        _context.SanitationTasks.Add(task);
        _context.SanitationLogs.Add(log);

        if (request.TargetType == SanitationTargetType.StorageLocation)
        {
            var location = await _context.StorageLocations
                .FirstOrDefaultAsync(l => l.Id == request.TargetId, cancellationToken);

            if (location != null)
            {
                location.CompleteSanitation(isPassed: true);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(log.Id);
    }
}