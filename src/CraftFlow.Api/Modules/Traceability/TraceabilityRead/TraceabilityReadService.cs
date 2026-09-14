using CraftFlow.Api.Modules.Traceability.Contracts;
using CraftFlow.SharedKernel.Constants;
using Dapper;
using System.Data;

namespace CraftFlow.Api.Modules.Traceability.TraceabilityRead;

public sealed class TraceabilityReadService
{
    private readonly IDbConnection _dbConnection;

    public TraceabilityReadService(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task<ForwardTraceabilityDto?> GetForwardTraceabilityAsync(Guid stockLotId, Guid tenantId)
    {
        var header = await _dbConnection.QueryFirstOrDefaultAsync<ForwardTraceabilityHeader>(
            TraceabilityConstants.SQL_GET_FORWARD_HEADER,
            new
            {
                StockLotId = stockLotId,
                TenantId = tenantId,
                DefaultMaterialName = FormattingConstants.CONST_DEFAULT_MATERIAL_NAME
            });

        if (header is null) return null;

        var batchRows = (await _dbConnection.QueryAsync<dynamic>(
            TraceabilityConstants.SQL_GET_FORWARD_BATCHES,
            new { StockLotId = stockLotId, TenantId = tenantId }))?.ToList() ?? new List<dynamic>();

        var batches = batchRows
            .Where(r => r.production_batch_id != null)
            .GroupBy(r => (Guid)r.production_batch_id)
            .Select(g =>
            {
                var first = g.First();
                return new TraceabilityProductionBatchDto(
                    g.Key,
                    Convert.ToInt32(first.batch_status_int ?? 0),
                    (DateTime?)(first.started_at) ?? DateTime.MinValue,
                    (DateTime?)first.completed_at,
                    0m,
                    0m,
                    0m,
                    new List<TraceabilityIngredientDto>()
                );
            }).ToList();

        return new ForwardTraceabilityDto(
            header.RawMaterialStockLotId,
            header.RawMaterialBatchNumber,
            header.RawMaterialName,
            batches
        );
    }

    public async Task<BackwardTraceabilityDto?> GetBackwardTraceabilityAsync(Guid productStockLotId, Guid tenantId)
    {
        var header = await _dbConnection.QueryFirstOrDefaultAsync<dynamic>(
            TraceabilityConstants.SQL_GET_BACKWARD_HEADER,
            new { ProductStockLotId = productStockLotId, TenantId = tenantId });

        if (header is null) return null;

        TraceabilityProductionBatchDto? originBatch = null;

        if (header.production_batch_id != null)
        {
            var rawIngredients = await _dbConnection.QueryAsync<dynamic>(
                TraceabilityConstants.SQL_GET_BACKWARD_CONSUMED,
                new { BatchId = (Guid)header.production_batch_id, TenantId = tenantId });

            var ingredients = rawIngredients?
                .Select(i => new TraceabilityIngredientDto(
                    (Guid)(i.raw_material_stock_lot_id ?? Guid.Empty),
                    (string)(i.raw_material_name ?? string.Empty),
                    (string)(i.batch_number ?? string.Empty),
                    Convert.ToDecimal(i.quantity_used ?? 0m),
                    (string)(i.unit_of_measure ?? string.Empty),
                    (string)(i.supplier_name ?? string.Empty)
                )).ToList() ?? [];

            decimal plannedQty = Convert.ToDecimal(header.planned_quantity ?? 0m);
            decimal brewOutputQty = Convert.ToDecimal(header.brew_output_quantity ?? 0m);

            decimal yieldPercentage = plannedQty > 0
                ? Math.Round((brewOutputQty / plannedQty) * 100m, 2)
                : 0m;

            int batchStatusInt = Convert.ToInt32(header.batch_status_int ?? 0);

            originBatch = new TraceabilityProductionBatchDto(
                (Guid)header.production_batch_id,
                batchStatusInt,
                (DateTime?)header.started_at ?? DateTime.MinValue,
                (DateTime?)header.completed_at,
                plannedQty,
                brewOutputQty,
                yieldPercentage,
                ingredients
            );
        }

        decimal currentStockQty = Convert.ToDecimal(header.actual_quantity ?? 0m);
        decimal brewOutput = Convert.ToDecimal(header.brew_output_quantity ?? 0m);
        int agingDays = Convert.ToInt32(header.aging_days ?? 0);
        decimal agingLossPercentage = 0m;
        if (agingDays > 0 && brewOutput > 0 && currentStockQty > 0 && currentStockQty < brewOutput)
        {
            agingLossPercentage = Math.Round(((brewOutput - currentStockQty) / brewOutput) * 100m, 2);
        }

        string chamberName = (string)(header.chamber_name ?? string.Empty);

        if (agingDays == 0 && string.IsNullOrEmpty(chamberName))
        {
            chamberName = FormattingConstants.CONST_DEFAULT_CHAMBER_NAME;
        }

        Guid? salesOrderId = (Guid?)header.sales_order_id;

        return new BackwardTraceabilityDto(
            salesOrderId,
            (string)(header.customer_name ?? FormattingConstants.CONST_DEFAULT_CUSTOMER_NAME),
            (Guid)header.product_stock_lot_id,
            (string)(header.product_batch_number ?? string.Empty),
            (string)(header.product_name ?? string.Empty),
            currentStockQty,
            Convert.ToDecimal(header.unit_price ?? 0m),
            agingDays,
            agingLossPercentage,
            chamberName,
            originBatch
        );
    }

    private record ForwardTraceabilityHeader(
        Guid RawMaterialStockLotId,
        string RawMaterialBatchNumber,
        string RawMaterialName
    );
}