using FluentValidation;

namespace CraftFlow.Api.Modules.Production.CompleteProductionBatch
{
    public class CompleteProductionBatchValidator : AbstractValidator<CompleteProductionBatchCommand>
    {
        public CompleteProductionBatchValidator()
        {
            RuleFor(x => x.BatchId).NotEmpty();
            RuleFor(x => x.ActualOutputQuantity).GreaterThan(0);
        }
    }
}
