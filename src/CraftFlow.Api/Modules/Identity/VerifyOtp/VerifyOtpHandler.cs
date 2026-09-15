using CraftFlow.Api.Common.Infrastructure.Identity;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Identity.LoginUser;
using CraftFlow.SharedKernel.Constants;
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

        if (user.TenantId != Guid.Empty)
        {
            var organization = await _dbContext.Organizations
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(o => o.Id == user.TenantId, cancellationToken);

            organization?.Activate();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var tokenString = _tokenService.GenerateJwtToken(user);

        return Result.Success(new LoginResponseDto(tokenString, user.TenantId, user.FullName, user.Email, user.IsAdmin));
    }
}