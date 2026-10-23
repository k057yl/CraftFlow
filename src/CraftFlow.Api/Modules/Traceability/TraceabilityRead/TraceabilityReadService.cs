using CraftFlow.SharedKernel.Dtos.Traceability;
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

        var batchRows = (await _dbConnection.QueryAsync<ForwardBatchRow>(
            TraceabilityConstants.SQL_GET_FORWARD_BATCHES,
            new { StockLotId = stockLotId, TenantId = tenantId }))?.ToList() ?? [];

        var batches = batchRows
            .Where(r => r.ProductionBatchId != Guid.Empty)
            .GroupBy(r => r.ProductionBatchId)
            .Select(g =>
            {
                var first = g.First();
                return new TraceabilityProductionBatchDto(
                    g.Key,
                    first.BatchStatusInt,
                    first.StartedAt ?? DateTime.MinValue,
                    first.CompletedAt,
                    first.PlannedQuantity,
                    first.ActualQuantity,
                    first.PlannedQuantity > 0 ? Math.Round((first.ActualQuantity / first.PlannedQuantity) * 100m, 2) : 0m,
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
        var header = await _dbConnection.QueryFirstOrDefaultAsync<BackwardHeaderRow>(
            TraceabilityConstants.SQL_GET_BACKWARD_HEADER,
            new { ProductStockLotId = productStockLotId, TenantId = tenantId });

        if (header is null) return null;

        TraceabilityProductionBatchDto? originBatch = null;

        if (header.ProductionBatchId.HasValue && header.ProductionBatchId.Value != Guid.Empty)
        {
            var rawIngredients = await _dbConnection.QueryAsync<BackwardIngredientRow>(
                TraceabilityConstants.SQL_GET_BACKWARD_CONSUMED,
                new { BatchId = header.ProductionBatchId.Value, TenantId = tenantId });

            var ingredients = rawIngredients?
                .Select(i => new TraceabilityIngredientDto(
                    i.RawMaterialStockLotId ?? Guid.Empty,
                    i.RawMaterialName ?? string.Empty,
                    i.BatchNumber ?? string.Empty,
                    i.QuantityUsed,
                    i.UnitOfMeasure ?? string.Empty,
                    i.SupplierId,
                    i.SupplierName ?? string.Empty
                )).ToList() ?? [];

            decimal plannedQty = header.PlannedQuantity;
            decimal brewOutputQty = header.BrewOutputQuantity;

            decimal yieldPercentage = plannedQty > 0
                ? Math.Round((brewOutputQty / plannedQty) * 100m, 2)
                : 0m;

            originBatch = new TraceabilityProductionBatchDto(
                header.ProductionBatchId.Value,
                header.BatchStatusInt,
                header.StartedAt ?? DateTime.MinValue,
                header.CompletedAt,
                plannedQty,
                brewOutputQty,
                yieldPercentage,
                ingredients
            );
        }

        decimal currentStockQty = header.ActualQuantity;
        decimal brewOutput = header.BrewOutputQuantity;
        int agingDays = header.AgingDays;
        decimal agingLossPercentage = 0m;

        if (agingDays > 0 && brewOutput > 0 && currentStockQty > 0 && currentStockQty < brewOutput)
        {
            agingLossPercentage = Math.Round(((brewOutput - currentStockQty) / brewOutput) * 100m, 2);
        }

        string chamberName = header.ChamberName ?? string.Empty;
        string locationName = header.LocationName ?? string.Empty;

        string displayLocation = !string.IsNullOrWhiteSpace(locationName)
            ? (!string.IsNullOrWhiteSpace(chamberName) ? $"{chamberName} ({locationName})" : locationName)
            : chamberName;

        if (agingDays == 0 && string.IsNullOrEmpty(displayLocation))
        {
            displayLocation = FormattingConstants.CONST_DEFAULT_CHAMBER_NAME;
        }

        return new BackwardTraceabilityDto(
            header.SalesOrderId,
            header.CustomerId,
            header.CustomerName ?? FormattingConstants.CONST_DEFAULT_CUSTOMER_NAME,
            header.ProductStockLotId,
            header.ProductBatchNumber ?? string.Empty,
            header.ProductName ?? string.Empty,
            currentStockQty,
            agingDays,
            agingLossPercentage,
            displayLocation,
            originBatch
        );
    }

    // Жесткие DTO для Dapper
    private sealed class ForwardTraceabilityHeader
    {
        public Guid RawMaterialStockLotId { get; set; }
        public string RawMaterialBatchNumber { get; set; } = string.Empty;
        public string RawMaterialName { get; set; } = string.Empty;
    }

    private sealed class ForwardBatchRow
    {
        public Guid ProductionBatchId { get; set; }
        public int BatchStatusInt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public decimal PlannedQuantity { get; set; }
        public decimal ActualQuantity { get; set; }
    }

    private sealed class BackwardHeaderRow
    {
        public Guid ProductStockLotId { get; set; }
        public string? ProductBatchNumber { get; set; }
        public string? ProductName { get; set; }
        public decimal ActualQuantity { get; set; }
        public Guid? ProductionBatchId { get; set; }
        public int BatchStatusInt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public decimal PlannedQuantity { get; set; }
        public decimal BrewOutputQuantity { get; set; }
        public int AgingDays { get; set; }
        public string? ChamberName { get; set; }
        public string? LocationName { get; set; }
        public Guid? SalesOrderId { get; set; }
        public Guid? CustomerId { get; set; }
        public string? CustomerName { get; set; }
    }

    private sealed class BackwardIngredientRow
    {
        public Guid? RawMaterialStockLotId { get; set; }
        public string? RawMaterialName { get; set; }
        public string? BatchNumber { get; set; }
        public decimal QuantityUsed { get; set; }
        public string? UnitOfMeasure { get; set; }
        public Guid? SupplierId { get; set; }
        public string? SupplierName { get; set; }
    }
}