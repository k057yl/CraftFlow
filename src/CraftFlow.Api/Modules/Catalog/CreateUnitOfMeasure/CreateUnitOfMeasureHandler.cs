using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Catalog.Domain;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Catalog.CreateUnitOfMeasure;

public class CreateUnitOfMeasureHandler : IRequestHandler<CreateUnitOfMeasureCommand, Result<Guid>>
{
    private readonly AppDbContext _dbContext;

    public CreateUnitOfMeasureHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(CreateUnitOfMeasureCommand request, CancellationToken cancellationToken)
    {
        var unit = UnitOfMeasure.Create(request.Name, request.Code);

        _dbContext.UnitsOfMeasure.Add(unit);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(unit.Id);
    }
}