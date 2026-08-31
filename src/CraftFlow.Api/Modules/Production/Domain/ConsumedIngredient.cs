using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Domain;

namespace CraftFlow.Api.Modules.Production.Domain;

public sealed class ConsumedIngredient : Entity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid ProductionBatchId { get; private set; }
    public Guid StockLotId { get; private set; }
    public Guid RawMaterialId { get; private set; }
    public decimal Quantity { get; private set; }

    private ConsumedIngredient() { }

    public static ConsumedIngredient Create(Guid productionBatchId, Guid stockLotId, Guid rawMaterialId, decimal quantity, Guid tenantId)
    {
        if (quantity <= 0)
            throw new ArgumentException(ErrorCodes.Production.CONSUMED_INGREDIENT_NEGATIVE_QUANTITY);

        return new ConsumedIngredient
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProductionBatchId = productionBatchId,
            StockLotId = stockLotId,
            RawMaterialId = rawMaterialId,
            Quantity = quantity
        };
    }
}