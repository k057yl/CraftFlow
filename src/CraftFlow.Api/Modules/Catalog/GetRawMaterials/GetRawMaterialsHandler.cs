using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Catalog.GetRawMaterials;

public class GetRawMaterialsHandler : IRequestHandler<GetRawMaterialsQuery, Result<List<RawMaterialDto>>>
{
    private readonly AppDbContext _dbContext;

    public GetRawMaterialsHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<RawMaterialDto>>> Handle(GetRawMaterialsQuery request, CancellationToken cancellationToken)
    {
        var materials = await _dbContext.RawMaterials
            .AsNoTracking()
            .Join(
                _dbContext.UnitsOfMeasure.AsNoTracking(),
                raw => raw.UnitOfMeasureId,
                uom => uom.Id,
                (raw, uom) => new RawMaterialDto(raw.Id, raw.Name, raw.UnitOfMeasureId, uom.Code)
            )
            .ToListAsync(cancellationToken);

        return Result.Success(materials);
    }
}