using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Dtos.Sanitation;
using CraftFlow.SharedKernel.Enums.Sanitation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Sanitation.GetSanitationDetails;

public sealed class GetSanitationDetailsQueryHandler : IRequestHandler<GetSanitationDetailsQuery, SanitationDetailsDto>
{
    private readonly AppDbContext _context;

    public GetSanitationDetailsQueryHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SanitationDetailsDto> Handle(GetSanitationDetailsQuery request, CancellationToken cancellationToken)
    {
        string targetName = "Объект";
        DateTime? lastWashedAt = null;
        DateTime? nextWashDueDate = null;

        if (request.TargetType == SanitationTargetType.StorageLocation)
        {
            var loc = await _context.StorageLocations.AsNoTracking().FirstOrDefaultAsync(l => l.Id == request.TargetId, cancellationToken);
            if (loc == null) throw new KeyNotFoundException(ErrorCodes.General.NOT_FOUND);
            targetName = loc.Name;
            lastWashedAt = loc.LastWashedAt;
            nextWashDueDate = loc.NextWashDueDate;
        }
        else if (request.TargetType == SanitationTargetType.Warehouse)
        {
            var wh = await _context.Warehouses.AsNoTracking().FirstOrDefaultAsync(w => w.Id == request.TargetId, cancellationToken);
            if (wh == null) throw new KeyNotFoundException(ErrorCodes.General.NOT_FOUND);
            targetName = wh.Name;
        }
        else if (request.TargetType == SanitationTargetType.AgingChamber)
        {
            var ch = await _context.AgingChambers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == request.TargetId, cancellationToken);
            if (ch == null) throw new KeyNotFoundException(ErrorCodes.General.NOT_FOUND);
            targetName = ch.Name;
        }

        var rule = await _context.SanitationRules.AsNoTracking()
            .FirstOrDefaultAsync(r => r.TargetType == request.TargetType && r.TargetId == request.TargetId, cancellationToken);

        var rawLogs = await _context.SanitationLogs.AsNoTracking()
            .Where(l => l.TargetType == request.TargetType && l.TargetId == request.TargetId)
            .OrderByDescending(l => l.CompletedAt)
            .ToListAsync(cancellationToken);

        var userIds = rawLogs.Select(l => l.PerformedByUserId)
            .Concat(rawLogs.Select(l => l.VerifiedByUserId))
            .Distinct();

        var users = await _context.Users.AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName, cancellationToken);

        var logDtos = rawLogs.Select(l => new SanitationLogItemDto(
            l.Id,
            users.TryGetValue(l.PerformedByUserId, out var pName) ? pName : "Сотрудник",
            users.TryGetValue(l.VerifiedByUserId, out var vName) ? vName : "Проверяющий",
            l.CompletedAt,
            l.VerifiedAt,
            l.DurationMinutes,
            l.ChemicalsUsed,
            l.Notes,
            l.IsPassed
        )).ToList();

        var lastLog = logDtos.FirstOrDefault();
        var lastWashedByUserName = lastLog?.PerformedByUserName ?? "—";

        string statusText = "🟢 В норме";
        if (nextWashDueDate.HasValue)
        {
            if (nextWashDueDate.Value < DateTime.UtcNow)
                statusText = "🚨 Просрочено";
            else if ((nextWashDueDate.Value - DateTime.UtcNow).TotalDays <= 2)
                statusText = "⚠️ Скоро мойка";
        }

        return new SanitationDetailsDto(
            request.TargetId,
            request.TargetType,
            targetName,
            rule?.TriggerType,
            rule?.IntervalDays,
            rule?.MaxBatchCount,
            rule?.RequiredChemicals,
            lastWashedAt ?? lastLog?.CompletedAt,
            lastWashedByUserName,
            nextWashDueDate,
            statusText,
            logDtos
        );
    }
}