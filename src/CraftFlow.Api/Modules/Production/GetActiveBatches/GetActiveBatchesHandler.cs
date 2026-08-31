using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Production.Domain;
using CraftFlow.SharedKernel.Constants;
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
                .ToListAsync(cancellationToken);

            var dtos = batches.Select(b =>
            {
                var batchIdStr = b.Id.ToString()[..8].ToUpper();
                var label = string.Concat(FormattingConstants.BATCH_PREFIX, batchIdStr, " (Plan: ", b.PlannedOutputQuantity, ")");

                return new ActiveBatchDto(b.Id, label);
            }).ToList();

            return Result.Success(dtos);
        }
    }
}