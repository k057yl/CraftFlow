using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Enums.Sanitation;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Sanitation.Tasks.CancelSanitationTask;

public sealed class CancelSanitationTaskCommandHandler : IRequestHandler<CancelSanitationTaskCommand, Result>
{
    private readonly AppDbContext _context;

    public CancelSanitationTaskCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(CancelSanitationTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.SanitationTasks
            .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);

        if (task == null)
            return Result.Failure(ErrorCodes.Sanitation.TASK_NOT_FOUND);

        task.Cancel(request.Reason);

        if (task.TargetType == SanitationTargetType.STORAGE_LOCATION)
        {
            var location = await _context.StorageLocations
                .FirstOrDefaultAsync(l => l.Id == task.TargetId, cancellationToken);

            if (location != null)
            {
                location.CompleteSanitation(isPassed: false);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}