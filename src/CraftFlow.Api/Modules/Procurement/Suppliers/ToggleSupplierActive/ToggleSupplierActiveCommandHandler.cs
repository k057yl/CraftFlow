using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Procurement.Suppliers.ToggleSupplierActive;

public class ToggleSupplierActiveCommandHandler : IRequestHandler<ToggleSupplierActiveCommand, Result>
{
    private readonly AppDbContext _dbContext;

    public ToggleSupplierActiveCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(ToggleSupplierActiveCommand request, CancellationToken cancellationToken)
    {
        if (request.UserRole != TenantRole.OWNER && request.UserRole != TenantRole.ACCOUNTANT)
        {
            return Result.Failure(Error.Forbidden(ErrorCodes.Auth.ACCESS_DENIED));
        }

        var supplier = await _dbContext.Suppliers
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (supplier == null)
        {
            return Result.Failure(Error.NotFound(ErrorCodes.Procurement.SUPPLIER_NOT_FOUND));
        }

        if (supplier.IsActive)
        {
            supplier.Deactivate();
        }
        else
        {
            supplier.Activate();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}