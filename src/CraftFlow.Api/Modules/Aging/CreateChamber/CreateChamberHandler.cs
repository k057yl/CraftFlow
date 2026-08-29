using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Aging.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Aging.CreateChamber;

public class CreateChamberHandler : IRequestHandler<CreateChamberCommand, Result<Guid>>
{
    private readonly AppDbContext _dbContext;

    public CreateChamberHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(CreateChamberCommand request, CancellationToken cancellationToken)
    {
        var nameExists = await _dbContext.AgingChambers
            .AnyAsync(c => c.Name == request.Name, cancellationToken);

        if (nameExists)
        {
            return Result.Failure<Guid>(Error.Conflict(ErrorCodes.General.ALREADY_EXISTS));
        }

        var chamber = AgingChamber.Create(
            request.Name,
            request.TargetTemperature,
            request.TargetHumidity
        );

        _dbContext.AgingChambers.Add(chamber);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(chamber.Id);
    }
}
