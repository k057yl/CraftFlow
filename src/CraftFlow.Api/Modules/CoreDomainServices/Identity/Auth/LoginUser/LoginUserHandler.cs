using CraftFlow.Api.Core.Infrastructure.Identity;
using CraftFlow.Api.Core.Persistence;
using CraftFlow.Api.Modules.CoreDomainServices.Identity.Domain;
using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.Services;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Dtos.Identity;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.CoreDomainServices.Identity.Auth.LoginUser;

public class LoginUserHandler : IRequestHandler<LoginUserCommand, Result<LoginResponseDto>>
{
    private readonly AppDbContext _dbContext;
    private readonly ITokenService _tokenService;
    private readonly SubscriptionLifecycleService _lifecycleService;

    public LoginUserHandler(
        AppDbContext dbContext,
        ITokenService tokenService,
        SubscriptionLifecycleService lifecycleService)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
        _lifecycleService = lifecycleService;
    }

    public async Task<Result<LoginResponseDto>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user == null)
        {
            return Result.Failure<LoginResponseDto>(Error.Validation(ErrorCodes.Auth.INVALID_CREDENTIALS));
        }

        if (!user.IsActive)
        {
            return Result.Failure<LoginResponseDto>(Error.Validation(ErrorCodes.Auth.ACCOUNT_NOT_ACTIVATED));
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Result.Failure<LoginResponseDto>(Error.Validation(ErrorCodes.Auth.INVALID_CREDENTIALS));
        }

        var isSystemAdmin = await _dbContext.SystemAdmins
            .AnyAsync(sa => sa.UserId == user.Id, cancellationToken);

        OrganizationMember? activeMember = null;

        if (!isSystemAdmin)
        {
            activeMember = await _dbContext.OrganizationMembers
                .IgnoreQueryFilters()
                .Include(m => m.Organization)
                .FirstOrDefaultAsync(m => m.UserId == user.Id && m.IsActive && m.Organization.IsActive, cancellationToken);

            if (activeMember == null)
            {
                return Result.Failure<LoginResponseDto>(Error.Validation(ErrorCodes.Auth.ACCOUNT_NOT_ACTIVATED));
            }

            await _lifecycleService.DowngradeExpiredModulesAsync(activeMember.TenantId, cancellationToken);
        }

        var tokenString = _tokenService.GenerateJwtToken(user, activeMember, isSystemAdmin);

        var tenantId = activeMember?.TenantId ?? Guid.Empty;
        var role = isSystemAdmin ? TenantRole.SUPER_ADMIN : (activeMember?.Role ?? TenantRole.NONE);

        return Result.Success(new LoginResponseDto(
            tokenString,
            tenantId,
            user.FullName,
            user.Email,
            role
        ));
    }
}