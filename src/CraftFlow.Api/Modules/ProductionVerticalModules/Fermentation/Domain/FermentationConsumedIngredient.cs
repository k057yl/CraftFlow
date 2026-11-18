using CraftFlow.SharedKernel.Domain;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.Fermentation.Domain;

public sealed class FermentationConsumedIngredient : Entity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid FermentationBatchId { get; private set; }
    public Guid StockLotId { get; private set; }
    public Guid RawMaterialId { get; private set; }
    public decimal Quantity { get; private set; }

    private FermentationConsumedIngredient() { }

    public static FermentationConsumedIngredient Create(
        Guid fermentationBatchId,
        Guid stockLotId,
        Guid rawMaterialId,
        decimal quantity,
        Guid tenantId)
    {
        return new FermentationConsumedIngredient
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FermentationBatchId = fermentationBatchId,
            StockLotId = stockLotId,
            RawMaterialId = rawMaterialId,
            Quantity = quantity
        };
    }
}