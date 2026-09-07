using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Identity.LoginUser;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CraftFlow.Api.Modules.Identity.VerifyOtp;

public class VerifyOtpHandler : IRequestHandler<VerifyOtpCommand, Result<LoginResponseDto>>
{
    private readonly AppDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public VerifyOtpHandler(AppDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task<Result<LoginResponseDto>> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
    {
        var sanitizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == sanitizedEmail, cancellationToken);

        if (user == null || string.IsNullOrEmpty(user.OtpCodeHash) || !user.OtpExpiresAtUtc.HasValue)
        {
            return Result.Failure<LoginResponseDto>(Error.Validation(ErrorCodes.Auth.INVALID_CREDENTIALS));
        }

        if (user.OtpExpiresAtUtc.Value < DateTime.UtcNow)
        {
            return Result.Failure<LoginResponseDto>(Error.Validation(ErrorCodes.Auth.INVALID_CREDENTIALS));
        }

        if (!BCrypt.Net.BCrypt.Verify(request.OtpCode, user.OtpCodeHash))
        {
            return Result.Failure<LoginResponseDto>(Error.Validation(ErrorCodes.Auth.INVALID_CREDENTIALS));
        }

        user.ClearOtpCode();
        await _dbContext.SaveChangesAsync(cancellationToken);

        var secretKey = _configuration["JWT_SECRET_KEY"]
            ?? _configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("JWT_SECRET_KEY_NOT_CONFIGURED");

        var key = Encoding.UTF8.GetBytes(secretKey);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("tenant_id", user.TenantId.ToString()),
            new Claim("full_name", user.FullName)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return Result.Success(new LoginResponseDto(tokenString, user.TenantId, user.FullName));
    }
}