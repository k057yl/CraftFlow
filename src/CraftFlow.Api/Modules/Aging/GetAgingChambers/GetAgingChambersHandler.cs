using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Aging.GetAgingChambers;
public class GetAgingChambersHandler : IRequestHandler<GetAgingChambersQuery, Result<List<AgingChamberDto>>>
{
    private readonly AppDbContext _dbContext;

    public GetAgingChambersHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<AgingChamberDto>>> Handle(GetAgingChambersQuery request, CancellationToken cancellationToken)
    {
        var chambers = await _dbContext.AgingChambers
            .AsNoTracking()
            .Select(c => new AgingChamberDto(c.Id, c.Name))
            .ToListAsync(cancellationToken);

        return Result.Success(chambers);
    }
}