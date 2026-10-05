using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Catalog.Domain;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Catalog.SeedUnitsOfMeasure;

public class SeedUnitsOfMeasureHandler : IRequestHandler<SeedUnitsOfMeasureCommand, Result<int>>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public SeedUnitsOfMeasureHandler(AppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<Result<int>> Handle(SeedUnitsOfMeasureCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        var existingCodes = await _dbContext.UnitsOfMeasure
            .Where(u => u.TenantId == tenantId)
            .Select(u => u.Code.ToLower())
            .ToListAsync(cancellationToken);

        if (existingCodes.Count > 0)
        {
            return Result.Success(0);
        }

        var kg = UnitOfMeasure.Create(
            NAME_KILOGRAM,
            CODE_KG,
            UnitType.Weight,
            CONVERSION_FACTOR_BASE
        );

        var g = UnitOfMeasure.Create(
            NAME_GRAM,
            CODE_G,
            UnitType.Weight,
            CONVERSION_FACTOR_GRAM,
            kg.Id
        );

        var l = UnitOfMeasure.Create(
            NAME_LITER,
            CODE_L,
            UnitType.Volume,
            CONVERSION_FACTOR_BASE
        );

        var ml = UnitOfMeasure.Create(
            NAME_MILLILITER,
            CODE_ML,
            UnitType.Volume,
            CONVERSION_FACTOR_MILLILITER,
            l.Id
        );

        var pcs = UnitOfMeasure.Create(
            NAME_PIECE,
            CODE_PCS,
            UnitType.Piece,
            CONVERSION_FACTOR_BASE
        );

        var itemsToSeed = new List<UnitOfMeasure> { kg, g, l, ml, pcs };

        _dbContext.UnitsOfMeasure.AddRange(itemsToSeed);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(itemsToSeed.Count);
    }

    private const string NAME_KILOGRAM = "Kilogram";
    private const string NAME_GRAM = "Gram";
    private const string NAME_LITER = "Liter";
    private const string NAME_MILLILITER = "Milliliter";
    private const string NAME_PIECE = "Piece";

    private const string CODE_KG = "kg";
    private const string CODE_G = "g";
    private const string CODE_L = "l";
    private const string CODE_ML = "ml";
    private const string CODE_PCS = "pcs";

    private const decimal CONVERSION_FACTOR_BASE = 1.0m;
    private const decimal CONVERSION_FACTOR_GRAM = 0.001m;
    private const decimal CONVERSION_FACTOR_MILLILITER = 0.001m;
}