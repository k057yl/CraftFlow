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
        var unit = await _dbContext.UnitsOfMeasure
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (unit is null)
            return Result.Failure(Error.NotFound(ErrorCodes.General.NOT_FOUND));

        unit.Archive();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}