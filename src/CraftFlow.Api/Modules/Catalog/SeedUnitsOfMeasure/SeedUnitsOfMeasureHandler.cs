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

        var itemsToSeed = GetPresetItems(request.Preset)
            .Where(item => !existingCodes.Contains(item.Code.ToLower()))
            .Select(item => UnitOfMeasure.Create(item.Name, item.Code))
            .ToList();

        if (itemsToSeed.Count == 0)
        {
            return Result.Success(0);
        }

        _dbContext.UnitsOfMeasure.AddRange(itemsToSeed);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(itemsToSeed.Count);
    }

    private static List<(string Name, string Code)> GetPresetItems(UomPreset preset) => preset switch
    {
        UomPreset.Metric => new()
        {
            ("Kilogram", "kg"),
            ("Gram", "g"),
            ("Liter", "l"),
            ("Milliliter", "ml"),
            ("Piece", "pcs")
        },
        UomPreset.Imperial => new()
        {
            ("Pound", "lb"),
            ("Ounce", "oz"),
            ("Gallon", "gal"),
            ("Fluid Ounce", "fl oz"),
            ("Piece", "pcs")
        },
        UomPreset.Full => new()
        {
            ("Kilogram", "kg"),
            ("Gram", "g"),
            ("Liter", "l"),
            ("Milliliter", "ml"),
            ("Piece", "pcs"),
            ("Pound", "lb"),
            ("Ounce", "oz"),
            ("Gallon", "gal")
        },
        _ => new()
    };
}