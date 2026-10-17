using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Enums.Sanitation;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Sanitation.Tasks.StartSanitationTask;

public sealed class StartSanitationTaskCommandHandler : IRequestHandler<StartSanitationTaskCommand, Result<Guid>>
{
    private readonly AppDbContext _context;

    public StartSanitationTaskCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(StartSanitationTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.SanitationTasks
            .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);

        if (task == null)
            return Result.Failure<Guid>(ErrorCodes.Sanitation.TASK_NOT_FOUND);

        task.Start(request.PerformerUserId, DateTime.UtcNow);

        if (task.TargetType == SanitationTargetType.StorageLocation)
        {
            var location = await _context.StorageLocations
                .FirstOrDefaultAsync(l => l.Id == task.TargetId, cancellationToken);

            if (location != null)
            {
                location.StartSanitation();
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(task.Id);
    }
}