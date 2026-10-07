using CraftFlow.Api.Common.Infrastructure.Identity;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Identity.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Dtos.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Identity.VerifyOtp;

public class VerifyOtpHandler : IRequestHandler<VerifyOtpCommand, Result<LoginResponseDto>>
{
    private readonly AppDbContext _dbContext;
    private readonly ITokenService _tokenService;

    public VerifyOtpHandler(AppDbContext dbContext, ITokenService tokenService)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
    }

    public async Task<Result<LoginResponseDto>> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var cleanOtpCode = request.OtpCode?.Trim() ?? string.Empty;

        var user = await _dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (user == null || string.IsNullOrEmpty(user.OtpCodeHash) || !user.OtpExpiresAtUtc.HasValue)
        {
            return Result.Failure<LoginResponseDto>(Error.Validation(ErrorCodes.Auth.INVALID_CREDENTIALS));
        }

        if (user.OtpExpiresAtUtc.Value < DateTime.UtcNow)
        {
            return Result.Failure<LoginResponseDto>(Error.Validation(ErrorCodes.Auth.OTP_EXPIRED));
        }

        if (!BCrypt.Net.BCrypt.Verify(cleanOtpCode, user.OtpCodeHash))
        {
            return Result.Failure<LoginResponseDto>(Error.Validation(ErrorCodes.Auth.INVALID_CREDENTIALS));
        }

        user.Activate();

        var member = await _dbContext.OrganizationMembers
            .IgnoreQueryFilters()
            .Include(m => m.Organization)
            .FirstOrDefaultAsync(m => m.UserId == user.Id, cancellationToken);

        if (member != null && member.Organization != null)
        {
            member.Organization.Activate();
            member.Activate();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var isSystemAdmin = await _dbContext.SystemAdmins
            .AnyAsync(sa => sa.UserId == user.Id, cancellationToken);

        var tokenString = _tokenService.GenerateJwtToken(user, member, isSystemAdmin);

        var tenantId = member?.TenantId ?? Guid.Empty;
        var role = isSystemAdmin ? TenantRole.SuperAdmin : (member?.Role ?? TenantRole.None);

        return Result.Success(new LoginResponseDto(tokenString, tenantId, user.FullName, user.Email, role));
    }
}