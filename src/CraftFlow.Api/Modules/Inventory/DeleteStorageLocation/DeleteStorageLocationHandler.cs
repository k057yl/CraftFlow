using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Inventory.DeleteStorageLocation;

public class DeleteStorageLocationHandler : IRequestHandler<DeleteStorageLocationCommand, Result<bool>>
{
    private readonly AppDbContext _dbContext;

    public DeleteStorageLocationHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<bool>> Handle(DeleteStorageLocationCommand request, CancellationToken cancellationToken)
    {
        var rowsAffected = await _dbContext.StorageLocations
            .Where(l => l.Id == request.Id)
            .ExecuteDeleteAsync(cancellationToken);

        if (rowsAffected == 0)
        {
            return Result.Failure<bool>(Error.NotFound(ErrorCodes.Inventory.LOCATION_NOT_FOUND));
        }

        return Result.Success(true);
    }
}