using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Inventory.Domain;
using CraftFlow.Api.Modules.Production.Domain;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Production.ConsumeIngredient;

public sealed class ConsumeIngredientCommandHandler : IRequestHandler<ConsumeIngredientCommand, Result<Guid>>
{
    private readonly AppDbContext _context;
    private readonly ITenantContext _tenantContext;

    public ConsumeIngredientCommandHandler(AppDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(ConsumeIngredientCommand request, CancellationToken cancellationToken)
    {
        var consumed = ConsumedIngredient.Create(
            request.ProductionBatchId,
            request.StockLotId,
            request.RawMaterialId,
            request.Quantity,
            _tenantContext.TenantId
        );

        await _context.Set<ConsumedIngredient>().AddAsync(consumed, cancellationToken);

        var stockLot = await _context.Set<StockLot>().FindAsync(new object[] { request.StockLotId }, cancellationToken);
        stockLot?.AdjustQuantity(-request.Quantity);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(consumed.Id);
    }
}