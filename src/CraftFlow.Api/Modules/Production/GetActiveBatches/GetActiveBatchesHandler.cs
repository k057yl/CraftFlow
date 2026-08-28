using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Production.Domain;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Production.GetActiveBatches
{
    public class GetActiveBatchesHandler : IRequestHandler<GetActiveBatchesQuery, Result<List<ActiveBatchDto>>>
    {
        private readonly AppDbContext _dbContext;

        public GetActiveBatchesHandler(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<List<ActiveBatchDto>>> Handle(GetActiveBatchesQuery request, CancellationToken cancellationToken)
        {
            var batches = await _dbContext.ProductionBatches
                .AsNoTracking()
                .Where(b => b.Status == BatchStatus.InProgress)
                .Select(b => new ActiveBatchDto(
                    b.Id,
                    "BATCH #" + b.Id.ToString().Substring(0, 8).ToUpper() + " (Plan: " + b.PlannedOutputQuantity + ")"))
                .ToListAsync(cancellationToken);

            return Result.Success(batches);
        }
    }
}
