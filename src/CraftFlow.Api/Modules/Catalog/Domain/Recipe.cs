using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Domain;

namespace CraftFlow.Api.Modules.Catalog.Domain;

public sealed class Recipe : AggregateRoot, ITenantEntity
{
    private readonly List<RecipeIngredient> _ingredients = [];

    public Guid TenantId { get; private set; }
    public Guid ProductId { get; private set; }
    public string Name { get; private set; } = null!;
    public decimal TargetOutputQuantity { get; private set; }
    public bool IsAgingRequired { get; private set; }
    public int? DefaultMinAgingDays { get; private set; }

    public IReadOnlyCollection<RecipeIngredient> Ingredients => _ingredients.AsReadOnly();

    private Recipe() { }

    public static Recipe Create(
        Guid productId,
        string name,
        decimal targetOutputQuantity,
        bool isAgingRequired = false,
        int? defaultMinAgingDays = null)
    {
        if (targetOutputQuantity <= 0)
            throw new ArgumentException(ErrorCodes.Catalog.RECIPE_INVALID_TARGET_OUTPUT);

        if (isAgingRequired && (defaultMinAgingDays is null or <= 0))
            throw new ArgumentException(ErrorCodes.Catalog.RECIPE_INVALID_AGING_DAYS);
        return new Recipe
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Name = name,
            TargetOutputQuantity = targetOutputQuantity,
            IsAgingRequired = isAgingRequired,
            DefaultMinAgingDays = isAgingRequired ? defaultMinAgingDays : null
        };
    }

    public void AddIngredient(Guid rawMaterialId, decimal quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException(ErrorCodes.Catalog.RECIPE_INVALID_INGREDIENT_QUANTITY);

        var existing = _ingredients.FirstOrDefault(i => i.RawMaterialId == rawMaterialId);
        if (existing != null)
        {
            existing.UpdateQuantity(existing.Quantity + quantity);
            return;
        }

        _ingredients.Add(new RecipeIngredient(Id, rawMaterialId, quantity));
    }

    public void RemoveIngredient(Guid rawMaterialId)
    {
        _ingredients.RemoveAll(i => i.RawMaterialId == rawMaterialId);
    }
}