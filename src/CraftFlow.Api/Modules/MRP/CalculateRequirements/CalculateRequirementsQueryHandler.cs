using System.Data;
using CraftFlow.Api.Common.Constants;
using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.SharedKernel.Dtos.MRP;
using CraftFlow.SharedKernel.Result;
using Dapper;
using MediatR;

namespace CraftFlow.Api.Modules.MRP.CalculateRequirements;

public sealed class FlatMrpRequirementRow
{
    public Guid BatchId { get; set; }
    public string BatchName { get; set; } = string.Empty;
    public string RecipeName { get; set; } = string.Empty;
    public decimal PlannedOutputQuantity { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public Guid RawMaterialId { get; set; }
    public string RawMaterialName { get; set; } = string.Empty;
    public decimal RequiredQuantity { get; set; }
    public decimal CurrentStockQuantity { get; set; }
}

public sealed class CalculateRequirementsQueryHandler : IRequestHandler<CalculateRequirementsQuery, Result<MrpReportDto>>
{
    private readonly IDbConnection _dbConnection;
    private readonly ITenantContext _tenantContext;

    public CalculateRequirementsQueryHandler(IDbConnection dbConnection, ITenantContext tenantContext)
    {
        _dbConnection = dbConnection;
        _tenantContext = tenantContext;
    }

    public async Task<Result<MrpReportDto>> Handle(CalculateRequirementsQuery request, CancellationToken cancellationToken)
    {
        if (request.BatchIds != null && request.BatchIds.Count == 0)
        {
            return Result.Success(new MrpReportDto(DateTime.UtcNow, [], []));
        }

        const string sql = """
            WITH batch_needs AS (
                SELECT 
                    pb."Id" AS BatchId,
                    pb."Name" AS BatchName,
                    r."Name" AS RecipeName,
                    pb."PlannedOutputQuantity" AS PlannedOutputQuantity,
                    pb."StartedAt" AS ScheduledAt,
                    ri."RawMaterialId" AS RawMaterialId,
                    COALESCE(rm."Name", @UnknownMaterialName) AS RawMaterialName,
                    SUM(
                        ri."Quantity" * (
                            CASE 
                                WHEN r."TargetOutputQuantity" > 0 THEN pb."PlannedOutputQuantity" / r."TargetOutputQuantity"
                                ELSE 1.0
                            END
                        )
                    ) AS RequiredQuantity
                FROM production.production_batches pb
                JOIN catalog.recipes r ON r."Id" = pb."RecipeId"
                JOIN catalog.recipe_ingredients ri ON ri."RecipeId" = r."Id"
                LEFT JOIN catalog.raw_materials rm ON rm."Id" = ri."RawMaterialId"
                WHERE pb."TenantId" = @TenantId
                  AND pb."State" IN (0, 1) -- БЕРЕМ И 0 (Draft), И 1 (Planned)
                  AND (@FilterByBatches = FALSE OR pb."Id" = ANY(@BatchIds))
                GROUP BY pb."Id", pb."Name", r."Name", pb."PlannedOutputQuantity", pb."StartedAt", ri."RawMaterialId", rm."Name"
            ),
            stock_summary AS (
                SELECT 
                    sl."ItemId" AS RawMaterialId,
                    SUM(sl."Quantity") AS CurrentStockQuantity
                FROM inventory.stock_lots sl
                WHERE sl."Quantity" > 0 
                  AND sl."TenantId" = @TenantId
                GROUP BY sl."ItemId"
            )
            SELECT 
                bn.BatchId AS BatchId,
                bn.BatchName AS BatchName,
                bn.RecipeName AS RecipeName,
                bn.PlannedOutputQuantity AS PlannedOutputQuantity,
                bn.ScheduledAt AS ScheduledAt,
                bn.RawMaterialId AS RawMaterialId,
                bn.RawMaterialName AS RawMaterialName,
                bn.RequiredQuantity AS RequiredQuantity,
                COALESCE(ss.CurrentStockQuantity, 0.0) AS CurrentStockQuantity
            FROM batch_needs bn
            LEFT JOIN stock_summary ss ON ss.RawMaterialId = bn.RawMaterialId;
            """;

        var command = new CommandDefinition(
            sql,
            new
            {
                TenantId = _tenantContext.TenantId,
                UnknownMaterialName = CoreConstants.Catalog.UNKNOWN_MATERIAL_NAME,
                FilterByBatches = request.BatchIds != null && request.BatchIds.Count > 0,
                BatchIds = request.BatchIds?.ToArray() ?? Array.Empty<Guid>()
            },
            cancellationToken: cancellationToken
        );

        var rows = (await _dbConnection.QueryAsync<FlatMrpRequirementRow>(command)).ToList();

        if (rows.Count == 0)
        {
            return Result.Success(new MrpReportDto(DateTime.UtcNow, [], []));
        }

        var batchGroups = rows
            .GroupBy(r => r.BatchId)
            .Select(g => new BatchMrpGroupDto(
                BatchId: g.Key,
                BatchName: g.First().BatchName,
                RecipeName: g.First().RecipeName,
                PlannedOutputQuantity: g.First().PlannedOutputQuantity,
                ScheduledAt: g.First().ScheduledAt,
                Ingredients: g.Select(i => new BatchMaterialRequirementDto(
                    RawMaterialId: i.RawMaterialId,
                    RawMaterialName: i.RawMaterialName,
                    TotalRequiredQuantity: i.RequiredQuantity,
                    CurrentStockQuantity: i.CurrentStockQuantity,
                    ShortageQuantity: Math.Max(0m, i.RequiredQuantity - i.CurrentStockQuantity)
                )).ToList()
            )).ToList();

        var summary = rows
            .GroupBy(r => r.RawMaterialId)
            .Select(g =>
            {
                var totalReq = g.Sum(x => x.RequiredQuantity);
                var stock = g.First().CurrentStockQuantity;
                return new SummaryRequirementDto(
                    RawMaterialId: g.Key,
                    RawMaterialName: g.First().RawMaterialName,
                    TotalRequiredQuantity: totalReq,
                    CurrentStockQuantity: stock,
                    ShortageQuantity: Math.Max(0m, totalReq - stock)
                );
            }).ToList();

        return Result.Success(new MrpReportDto(DateTime.UtcNow, batchGroups, summary));
    }
}