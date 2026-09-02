using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Aging.Domain;
using CraftFlow.Api.Modules.Inventory.Domain;
using CraftFlow.Api.Modules.Production.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Aging.ReleaseFromAging;

public sealed class ReleaseFromAgingCommandHandler : IRequestHandler<ReleaseFromAgingCommand, Result>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public ReleaseFromAgingCommandHandler(AppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(ReleaseFromAgingCommand request, CancellationToken cancellationToken)
    {
        var lot = await _dbContext.Set<AgingLot>()
            .FirstOrDefaultAsync(l => l.Id == request.AgingLotId, cancellationToken);

        if (lot is null)
        {
            return Result.Failure(Error.NotFound(ErrorCodes.General.NOT_FOUND));
        }

        lot.RegisterWeightLoss(request.ActualFinalQuantity);
        lot.Release();

        var batch = await _dbContext.Set<ProductionBatch>()
            .FirstOrDefaultAsync(b => b.Id == lot.ProductionBatchId, cancellationToken);

        if (batch != null)
        {
            batch.ReleaseFromAging(request.ActualFinalQuantity);
        }

        var cleanBatchName = lot.BatchNumber.Contains("(Выход:")
            ? lot.BatchNumber.Substring(0, lot.BatchNumber.IndexOf("(Выход:")).Trim()
            : lot.BatchNumber;

        var finalBatchNumber = $"{cleanBatchName} (Выход: {request.ActualFinalQuantity:N2} кг)";

        var stockLot = StockLot.Create(
            warehouseId: request.TargetWarehouseId,
            itemId: lot.ProductId,
            initialQuantity: request.ActualFinalQuantity,
            unitPrice: request.UnitPrice,
            batchNumber: finalBatchNumber,
            tenantId: _tenantContext.TenantId,
            productionBatchId: lot.ProductionBatchId
        );

        await _dbContext.Set<StockLot>().AddAsync(stockLot, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}