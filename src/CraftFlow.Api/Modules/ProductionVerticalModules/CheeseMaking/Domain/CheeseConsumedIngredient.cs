using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Domain;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Domain;

public sealed class CheeseConsumedIngredient : Entity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public Guid CheeseBatchId { get; private set; }
    public Guid StockLotId { get; private set; }
    public Guid RawMaterialId { get; private set; }
    public decimal Quantity { get; private set; }

    private CheeseConsumedIngredient() { }

    public static CheeseConsumedIngredient Create(Guid cheeseBatchId, Guid stockLotId, Guid rawMaterialId, decimal quantity, Guid tenantId)
    {
        if (quantity <= 0)
            throw new ArgumentException(ErrorCodes.Production.CONSUMED_INGREDIENT_NEGATIVE_QUANTITY);

        return new CheeseConsumedIngredient
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CheeseBatchId = cheeseBatchId,
            StockLotId = stockLotId,
            RawMaterialId = rawMaterialId,
            Quantity = quantity
        };
    }
}