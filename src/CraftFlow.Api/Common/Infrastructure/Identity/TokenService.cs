using CraftFlow.Api.Modules.Identity.Domain;
using CraftFlow.SharedKernel.Constants;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CraftFlow.Api.Common.Infrastructure.Identity;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateJwtToken(User user)
    {
        var adminEmail = Environment.GetEnvironmentVariable(AuthConstants.ADMIN_CONFIG_KEYS.ADMIN_EMAIL_KEY);
        var isAdmin = user.IsAdmin || (!string.IsNullOrEmpty(adminEmail) &&
                      user.Email.Equals(adminEmail.Trim().ToLowerInvariant(), StringComparison.OrdinalIgnoreCase));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(AuthConstants.Claims.TENANT_ID, user.TenantId.ToString()),
            new(AuthConstants.Claims.FULL_NAME, user.FullName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (isAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, AuthConstants.Roles.ADMIN));
            claims.Add(new Claim(AuthConstants.Claims.ROLE_SHORT, AuthConstants.Roles.ADMIN));
        }
        else
        {
            claims.Add(new Claim(ClaimTypes.Role, AuthConstants.Roles.USER));
            claims.Add(new Claim(AuthConstants.Claims.ROLE_SHORT, AuthConstants.Roles.USER));
        }

        var secretKey = _config["Jwt:SecretKey"]
            ?? _config["JWT_SECRET_KEY"]
            ?? throw new InvalidOperationException("JWT_SECRET_KEY_NOT_CONFIGURED");

        var issuer = _config["Jwt:Issuer"]?.Trim();
        var audience = _config["Jwt:Audience"]?.Trim();

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        return Convert.ToBase64String(randomNumber)
            .Replace('+', '-')
            .Replace('/', '_')
            .Replace("=", "");
    }

    public string HashRefreshToken(string refreshToken)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(refreshToken);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var secretKey = _config["Jwt:SecretKey"]
            ?? _config["JWT_SECRET_KEY"]
            ?? throw new InvalidOperationException("JWT_SECRET_KEY_NOT_CONFIGURED");

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("INVALID_TOKEN");
        }

        return principal;
    }
}