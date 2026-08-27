using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Catalog.GetRawMaterials
{
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
                .Select(r => new RawMaterialDto(r.Id, r.Name, r.UnitOfMeasureId))
                .ToListAsync(cancellationToken);

            return Result.Success(materials);
        }
    }
}
