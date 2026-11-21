using CraftFlow.Api.Core.MultiTenancy;
using CraftFlow.Api.Core.Persistence;
using CraftFlow.Api.Modules.CoreDomainServices.Inventory.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.CoreDomainServices.Inventory.StockLots.ReceiveStockBatch;

public sealed class ReceiveStockBatchHandler : IRequestHandler<ReceiveStockBatchCommand, Result<List<Guid>>>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public ReceiveStockBatchHandler(AppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<Result<List<Guid>>> Handle(ReceiveStockBatchCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.IsResolved || _tenantContext.TenantId == Guid.Empty)
            return Result.Failure<List<Guid>>(Error.Unauthorized(ErrorCodes.Tenant.INVALID_TENANT_ID));

        if (request.Items == null || request.Items.Count == 0)
            return Result.Failure<List<Guid>>(Error.Validation(ErrorCodes.General.VALUE_REQUIRED));

        var itemIds = request.Items.Select(i => i.ItemId).Distinct().ToList();

        var rawUoms = await _dbContext.RawMaterials
            .Where(r => itemIds.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id, r => r.UnitOfMeasureId, cancellationToken);

        var productUoms = await _dbContext.Products
            .Where(p => itemIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.UnitOfMeasureId, cancellationToken);

        var allLocationIds = request.Items
            .Where(i => i.StorageLocationIds != null)
            .SelectMany(i => i.StorageLocationIds!)
            .Distinct()
            .ToList();

        var locationsMap = allLocationIds.Count > 0
            ? await _dbContext.StorageLocations
                .Where(l => allLocationIds.Contains(l.Id))
                .ToDictionaryAsync(l => l.Id, cancellationToken)
            : new Dictionary<Guid, StorageLocation>();

        var createdIds = new List<Guid>(request.Items.Count);

        foreach (var line in request.Items)
        {
            if (line.Quantity <= 0 || line.UnitPrice < 0)
                return Result.Failure<List<Guid>>(Error.Validation(ErrorCodes.General.VALUE_REQUIRED));

            var finalUomId = line.UnitOfMeasureId;
            if (finalUomId == Guid.Empty)
            {
                if (rawUoms.TryGetValue(line.ItemId, out var rUom))
                    finalUomId = rUom;
                else if (productUoms.TryGetValue(line.ItemId, out var pUom))
                    finalUomId = pUom;
                else
                    return Result.Failure<List<Guid>>(Error.NotFound(ErrorCodes.Catalog.UNIT_OF_MEASURE_NOT_FOUND));
            }

            DateTime? utcExpirationDate = line.ExpirationDate.HasValue
                ? DateTime.SpecifyKind(line.ExpirationDate.Value, DateTimeKind.Utc)
                : null;

            var batchNumber = string.IsNullOrWhiteSpace(line.BatchNumber)
                ? request.InvoiceNumber
                : line.BatchNumber;

            var stockLot = StockLot.Create(
                tenantId: _tenantContext.TenantId,
                warehouseId: request.WarehouseId,
                itemId: line.ItemId,
                unitOfMeasureId: finalUomId,
                initialQuantity: line.Quantity,
                unitsCount: 1,
                unitPrice: line.UnitPrice,
                batchNumber: batchNumber,
                supplierId: request.SupplierId,
                expirationDate: utcExpirationDate
            );

            if (line.StorageLocationIds != null && line.StorageLocationIds.Count > 0)
            {
                decimal remainingQuantity = line.Quantity;

                foreach (var locId in line.StorageLocationIds)
                {
                    if (remainingQuantity <= 0) break;
                    if (!locationsMap.TryGetValue(locId, out var location)) continue;

                    decimal freeCapacity = location.Capacity.HasValue
                        ? Math.Max(0, location.Capacity.Value - location.CurrentVolume)
                        : remainingQuantity;

                    decimal fillAmount = Math.Min(remainingQuantity, freeCapacity);

                    if (fillAmount > 0)
                    {
                        location.AddVolume(fillAmount);
                        location.RegisterBatchProcessed();
                        stockLot.AssignStorageLocation(location.Id, fillAmount);
                        remainingQuantity -= fillAmount;
                    }
                }
            }

            _dbContext.StockLots.Add(stockLot);
            createdIds.Add(stockLot.Id);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(createdIds);
    }
}