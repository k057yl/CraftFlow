using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Sanitation.CompleteSanitationTask;

public sealed class CompleteSanitationTaskCommandHandler : IRequestHandler<CompleteSanitationTaskCommand, Result<Guid>>
{
    private readonly AppDbContext _context;

    public CompleteSanitationTaskCommandHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CompleteSanitationTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _context.SanitationTasks
            .FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);

        if (task == null)
            return Result.Failure<Guid>(ErrorCodes.Sanitation.TASK_NOT_FOUND);

        task.Complete(DateTime.UtcNow, request.ChemicalsUsed, request.Notes);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(task.Id);
    }
}