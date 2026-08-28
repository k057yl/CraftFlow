using FluentValidation;

namespace CraftFlow.Api.Modules.Production.StartProductionBatch
{
    public class StartProductionBatchValidator : AbstractValidator<StartProductionBatchCommand>
    {
        public StartProductionBatchValidator()
        {
            RuleFor(x => x.RecipeId).NotEmpty();
            RuleFor(x => x.WarehouseId).NotEmpty();
            RuleFor(x => x.PlannedOutputQuantity).GreaterThan(0);
        }
    }
}
