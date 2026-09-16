using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Identity.DeactivateAccount;
public class DeactivateAccountHandler : IRequestHandler<DeactivateAccountCommand, Result<bool>>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public DeactivateAccountHandler(AppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<Result<bool>> Handle(DeactivateAccountCommand request, CancellationToken cancellationToken)
    {
        var userId = _tenantContext.UserId;
        var tenantId = _tenantContext.TenantId;

        var user = await _dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null || !string.Equals(user.Email.Trim(), request.ConfirmationEmail?.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<bool>(Error.Validation("CONFIRMATION_EMAIL_MISMATCH"));
        }

        var org = await _dbContext.Organizations
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(o => o.Id == tenantId, cancellationToken);

        if (org != null)
        {
            org.DeactivateByOwner();
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return Result.Success(true);
    }
}
