using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Catalog.DeleteUnitOfMeasure;

public class DeleteUnitOfMeasureHandler : IRequestHandler<DeleteUnitOfMeasureCommand, Result>
{
    private readonly AppDbContext _dbContext;

    public DeleteUnitOfMeasureHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(DeleteUnitOfMeasureCommand request, CancellationToken cancellationToken)
    {
        var rowsAffected = await _dbContext.UnitsOfMeasure
            .Where(u => u.Id == request.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.IsActive, false), cancellationToken);

        if (rowsAffected == 0)
        {
            return Result.Failure(Error.NotFound(ErrorCodes.General.NOT_FOUND));
        }

        return Result.Success();
    }
}