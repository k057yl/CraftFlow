using FluentValidation;

namespace CraftFlow.Api.Modules.Sales.CreateCustomer
{
    public class CreateCustomerValidator : AbstractValidator<CreateCustomerCommand>
    {
        public CreateCustomerValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.Phone)
                .MaximumLength(50)
                .When(x => !string.IsNullOrEmpty(x.Phone));
        }
    }
}
