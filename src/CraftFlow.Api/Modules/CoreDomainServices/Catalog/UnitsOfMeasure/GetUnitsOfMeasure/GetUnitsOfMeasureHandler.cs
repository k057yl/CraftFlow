using CraftFlow.Api.Core.Persistence;
using CraftFlow.SharedKernel.Dtos.Catalog.Nomenclature;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.CoreDomainServices.Catalog.UnitsOfMeasure.GetUnitsOfMeasure
{
    public class GetUnitsOfMeasureHandler : IRequestHandler<GetUnitsOfMeasureQuery, Result<List<UnitOfMeasureDto>>>
    {
        private readonly AppDbContext _dbContext;

        public GetUnitsOfMeasureHandler(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<List<UnitOfMeasureDto>>> Handle(GetUnitsOfMeasureQuery request, CancellationToken cancellationToken)
        {
            var units = await _dbContext.UnitsOfMeasure
                .AsNoTracking()
                .Select(u => new UnitOfMeasureDto(u.Id, u.Name, u.Code))
                .ToListAsync(cancellationToken);

            return Result.Success(units);
        }
    }
}
