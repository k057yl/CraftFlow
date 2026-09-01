using CraftFlow.Api.Modules.Production.Domain;
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
            .GroupBy(r => (Guid)r.production_batch_id)
            .Select(g => new TraceabilityProductionBatchDto(
                g.Key,
                ((BatchStatus)(int)g.First().batch_status_int).ToString(),
                (DateTime?)g.First().started_at ?? DateTime.MinValue,
                (DateTime?)g.First().completed_at,
                g.Where(r => r.aging_lot_id != null)
                 .Select(r => new TraceabilityAgingLotDto(
                     (Guid)r.aging_lot_id,
                     (string)r.aging_batch_number,
                     (string)(r.chamber_name ?? FormattingConstants.CONST_DEFAULT_CHAMBER_NAME),
                     (string)r.aging_status
                 )).ToList() ?? new List<TraceabilityAgingLotDto>(),
                new List<TraceabilityIngredientDto>()
            )).ToList() ?? new List<TraceabilityProductionBatchDto>();

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

        if (header is null || header.production_batch_id == null) return null;

        var rawIngredients = await _dbConnection.QueryAsync<dynamic>(
            TraceabilityConstants.SQL_GET_BACKWARD_CONSUMED,
            new { BatchId = (Guid)header.production_batch_id, TenantId = tenantId });

        var ingredients = rawIngredients?
            .Select(i => new TraceabilityIngredientDto(
                (Guid)i.raw_material_stock_lot_id,
                (string)i.raw_material_name,
                (string)i.batch_number,
                (decimal)i.quantity_used
            )).ToList() ?? new List<TraceabilityIngredientDto>();

        var originBatch = new TraceabilityProductionBatchDto(
            (Guid)header.production_batch_id,
            ((BatchStatus)(int)header.batch_status_int).ToString(),
            (DateTime?)header.started_at ?? DateTime.MinValue,
            (DateTime?)header.completed_at,
            new List<TraceabilityAgingLotDto>(),
            ingredients
        );

        return new BackwardTraceabilityDto(
            header.sales_order_id != null ? (Guid)header.sales_order_id : Guid.Empty,
            header.customer_name ?? FormattingConstants.CONST_DEFAULT_CUSTOMER_NAME,
            (Guid)header.product_stock_lot_id,
            (string)header.product_batch_number,
            (string)header.product_name,
            originBatch
        );
    }

    private record ForwardTraceabilityHeader(
        Guid RawMaterialStockLotId,
        string RawMaterialBatchNumber,
        string RawMaterialName
    );
}