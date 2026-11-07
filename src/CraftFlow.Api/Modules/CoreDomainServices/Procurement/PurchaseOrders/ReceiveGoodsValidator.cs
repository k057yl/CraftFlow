using CraftFlow.Api.Core.Persistence;
using CraftFlow.SharedKernel.Constants;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.CoreDomainServices.Procurement.PurchaseOrders;

public sealed class ReceiveGoodsValidator : AbstractValidator<ReceiveGoodsCommand>
{
    public ReceiveGoodsValidator(AppDbContext dbContext)
    {
        RuleFor(x => x.PurchaseOrderId)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.General.VALUE_REQUIRED)
            .MustAsync(async (id, ct) => await dbContext.PurchaseOrders.AnyAsync(po => po.Id == id, ct))
            .WithErrorCode(ErrorCodes.Procurement.PURCHASE_ORDER_NOT_FOUND);
    }
}