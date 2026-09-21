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
        var location = await _dbContext.StorageLocations
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

        if (location == null)
        {
            return Result.Failure<bool>(Error.NotFound(ErrorCodes.Inventory.LOCATION_NOT_FOUND));
        }

        _dbContext.StorageLocations.Remove(location);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}