using CraftFlow.SharedKernel.Constants;
using FluentValidation;

namespace CraftFlow.Api.Modules.Procurement.PurchaseOrders;

public sealed class ReceiveGoodsCommandValidator : AbstractValidator<ReceiveGoodsCommand>
{
    public ReceiveGoodsCommandValidator()
    {
        RuleFor(x => x.PurchaseOrderId)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.General.VALUE_REQUIRED);
    }
}