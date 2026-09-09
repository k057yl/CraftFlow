using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static CraftFlow.SharedKernel.Constants.AuthConstants;

namespace CraftFlow.Api.Modules.Identity.LoginUser;

public class LoginUserHandler : IRequestHandler<LoginUserCommand, Result<LoginResponseDto>>
{
    private readonly AppDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public LoginUserHandler(AppDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task<Result<LoginResponseDto>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Result.Failure<LoginResponseDto>(Error.Validation(ErrorCodes.Auth.INVALID_CREDENTIALS));
        }

        var secretKey = _configuration["JWT_SECRET_KEY"]
            ?? _configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("JWT_SECRET_KEY_NOT_CONFIGURED");

        var key = Encoding.UTF8.GetBytes(secretKey);

        var adminEmail = Environment.GetEnvironmentVariable(ADMIN_CONFIG_KEYS.ADMIN_EMAIL_KEY);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("tenant_id", user.TenantId.ToString()),
            new Claim("full_name", user.FullName)
        };

        if (!string.IsNullOrEmpty(adminEmail) && email.Equals(adminEmail.Trim().ToLowerInvariant(), StringComparison.OrdinalIgnoreCase))
        {
            claims.Add(new Claim(SYSTEM_SECURITY_CONSTANTS.ADMIN_ROLE_CLAIM_KEY, SYSTEM_SECURITY_CONSTANTS.ADMIN_ROLE_VALUE));
        }

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