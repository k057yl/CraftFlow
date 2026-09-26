using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Procurement.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.MRP.CreateProcurement;

public class CreateProcurementFromMrpHandler : IRequestHandler<CreateProcurementFromMrpCommand, Result<Guid>>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public CreateProcurementFromMrpHandler(AppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(CreateProcurementFromMrpCommand request, CancellationToken cancellationToken)
    {
        if (request.SupplierId == Guid.Empty || request.WarehouseId == Guid.Empty || request.Items.Count == 0)
        {
            return Result.Failure<Guid>(Error.Validation(ErrorCodes.General.VALUE_REQUIRED));
        }

        var purchaseOrder = PurchaseOrder.Create(_tenantContext.TenantId, request.SupplierId, request.WarehouseId);

        foreach (var item in request.Items.Where(i => i.Quantity > 0))
        {
            decimal estimatedPrice = 100.00m;
            purchaseOrder.AddItem(item.RawMaterialId, item.Quantity, estimatedPrice);
        }

        _dbContext.PurchaseOrders.Add(purchaseOrder);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(purchaseOrder.Id);
    }
}