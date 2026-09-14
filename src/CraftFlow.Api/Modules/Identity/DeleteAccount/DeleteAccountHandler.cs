using CraftFlow.Api.Common.MultiTenancy;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Identity.DeleteAccount;

public class DeleteAccountHandler : IRequestHandler<DeleteAccountCommand, Result<bool>>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public DeleteAccountHandler(AppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<Result<bool>> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var userId = _tenantContext.UserId;

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            return Result.Failure<bool>(Error.NotFound(ErrorCodes.Auth.USER_NOT_FOUND));
        }

        if (!string.Equals(user.Email.Trim(), request.ConfirmationEmail?.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<bool>(Error.Validation("CONFIRMATION_EMAIL_MISMATCH"));
        }

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}