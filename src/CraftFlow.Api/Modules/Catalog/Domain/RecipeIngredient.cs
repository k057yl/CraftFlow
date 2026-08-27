using CraftFlow.SharedKernel.Domain;

namespace CraftFlow.Api.Modules.Catalog.Domain
{
    public sealed class RecipeIngredient : Entity
    {
        public Guid RecipeId { get; private set; }
        public Guid RawMaterialId { get; private set; }
        public decimal Quantity { get; private set; }

        internal RecipeIngredient(Guid recipeId, Guid rawMaterialId, decimal quantity)
        {
            Id = Guid.NewGuid();
            RecipeId = recipeId;
            RawMaterialId = rawMaterialId;
            Quantity = quantity;
        }

        internal void UpdateQuantity(decimal newQuantity) => Quantity = newQuantity;

        private RecipeIngredient() { }
    }
}
