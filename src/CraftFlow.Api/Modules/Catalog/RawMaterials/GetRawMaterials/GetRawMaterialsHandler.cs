using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Dtos.Catalog.Nomenclature;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Catalog.RawMaterials.GetRawMaterials;

public class GetRawMaterialsHandler : IRequestHandler<GetRawMaterialsQuery, Result<List<RawMaterialDto>>>
{
    private readonly AppDbContext _dbContext;

    public GetRawMaterialsHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<RawMaterialDto>>> Handle(GetRawMaterialsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.RawMaterials.AsNoTracking();

        if (!request.IncludeArchived)
        {
            query = query.Where(raw => raw.IsActive);
        }

        var materials = await query
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