using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Aging.DeleteChamber;

public class DeleteChamberHandler : IRequestHandler<DeleteChamberCommand, Result<bool>>
{
    private readonly AppDbContext _dbContext;

    public DeleteChamberHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<bool>> Handle(DeleteChamberCommand request, CancellationToken cancellationToken)
    {
        var rowsAffected = await _dbContext.AgingChambers
            .Where(c => c.Id == request.Id)
            .ExecuteDeleteAsync(cancellationToken);

        if (rowsAffected == 0)
        {
            return Result.Failure<bool>(Error.NotFound(ErrorCodes.General.NOT_FOUND));
        }

        return Result.Success(true);
    }
}