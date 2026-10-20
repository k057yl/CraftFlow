using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Sanitation.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Enums.Sanitation;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Sanitation.Tasks.VerifySanitationTask;

public sealed class VerifySanitationTaskCommandHandler : IRequestHandler<VerifySanitationTaskCommand, Result<Guid>>
{
    private readonly AppDbContext _context;

    public VerifySanitationTaskCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(VerifySanitationTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.SanitationTasks
            .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);

        if (task == null)
            return Result.Failure<Guid>(ErrorCodes.Sanitation.TASK_NOT_FOUND);

        var log = task.Verify(request.VerifierUserId, DateTime.UtcNow, request.IsPassed, request.VerificationNotes);
        _context.SanitationLogs.Add(log);

        if (task.TargetType == SanitationTargetType.STORAGE_LOCATION)
        {
            var location = await _context.StorageLocations
                .FirstOrDefaultAsync(l => l.Id == task.TargetId, cancellationToken);

            if (location != null)
            {
                location.CompleteSanitation(request.IsPassed);
            }
        }

        if (request.IsPassed && task.RuleId.HasValue)
        {
            var rule = await _context.SanitationRules
                .FirstOrDefaultAsync(r => r.Id == task.RuleId.Value, cancellationToken);

            if (rule != null && rule.IntervalDays.HasValue)
            {
                var nextDate = DateTime.UtcNow.AddDays(rule.IntervalDays.Value);
                var nextTask = SanitationTask.Create(
                    task.TenantId,
                    task.TargetType,
                    task.TargetId,
                    nextDate,
                    rule.Id
                );
                _context.SanitationTasks.Add(nextTask);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(log.Id);
    }
}