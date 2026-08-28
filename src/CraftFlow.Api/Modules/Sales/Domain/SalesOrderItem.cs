using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Domain;

namespace CraftFlow.Api.Modules.Sales.Domain
{
    public sealed class SalesOrderItem : Entity
    {
        public Guid SalesOrderId { get; private set; }
        public Guid ProductId { get; private set; }
        public decimal Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }

        private SalesOrderItem() { }

        public static SalesOrderItem Create(Guid productId, decimal quantity, decimal unitPrice)
        {
            if (quantity <= 0 || unitPrice < 0)
                throw new ArgumentException(ErrorCodes.General.VALUE_REQUIRED);

            return new SalesOrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                Quantity = quantity,
                UnitPrice = unitPrice
            };
        }
    }
}
