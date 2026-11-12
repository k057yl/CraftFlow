using CraftFlow.Api.Core.Persistence;
using CraftFlow.Api.Modules.CoreDomainServices.Identity.Domain;
using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.Domain;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Enums;
using CraftFlow.SharedKernel.Enums.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CraftFlow.Api.Core.Infrastructure.Identity;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    private readonly AppDbContext _dbContext;

    public TokenService(IConfiguration config, AppDbContext dbContext)
    {
        _config = config;
        _dbContext = dbContext;
    }

    public string GenerateJwtToken(User user, OrganizationMember? member = null, bool isSystemAdmin = false)
    {
        var effectiveRole = isSystemAdmin
            ? TenantRole.SUPER_ADMIN
            : (member?.Role ?? TenantRole.NONE);

        var tenantId = isSystemAdmin
            ? Guid.Empty
            : (member?.TenantId ?? Guid.Empty);

        var activeModules = AppModule.NONE;

        if (isSystemAdmin)
        {
            activeModules = AppModule.CHEESE_MAKING | AppModule.BREWERY | AppModule.MEAT_PROCESSING | AppModule.FERMENTATION;
        }
        else if (tenantId != Guid.Empty)
        {
            var activeSubscriptions = _dbContext.TenantSubscriptions
                .IgnoreQueryFilters()
                .Include(s => s.Plan)
                .AsNoTracking()
                .Where(s => s.TenantId == tenantId &&
                            (s.State == SubscriptionState.Active || s.State == SubscriptionState.Trial) &&
                            (!s.ExpiresAtUtc.HasValue || s.ExpiresAtUtc > DateTime.UtcNow))
                .ToList();

            foreach (var sub in activeSubscriptions)
            {
                if (sub.Plan != null)
                {
                    activeModules |= sub.Plan.TargetModule;
                }
            }
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(AuthConstants.Claims.TENANT_ID, tenantId.ToString()),
            new(AuthConstants.Claims.FULL_NAME, user.FullName),
            new(ClaimTypes.Role, effectiveRole.ToString()),
            new(AuthConstants.Claims.ROLE_ID, ((int)effectiveRole).ToString()),
            new(AuthConstants.Claims.ROLE_SHORT, effectiveRole.ToString()),
            new("modules", ((int)activeModules).ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var secretKey = _config[AuthConstants.ConfigurationKeys.JWT_SECRET_KEY_PATH]
            ?? _config[AuthConstants.ConfigurationKeys.JWT_SECRET_KEY_ENV]
            ?? throw new InvalidOperationException(AuthConstants.ErrorMessages.JWT_SECRET_KEY_NOT_CONFIGURED);

        var issuer = _config[AuthConstants.ConfigurationKeys.JWT_ISSUER_PATH]?.Trim();
        var audience = _config[AuthConstants.ConfigurationKeys.JWT_AUDIENCE_PATH]?.Trim();

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(AuthConstants.Token.DEFAULT_EXPIRATION_DAYS),
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
        var secretKey = _config[AuthConstants.ConfigurationKeys.JWT_SECRET_KEY_PATH]
            ?? _config[AuthConstants.ConfigurationKeys.JWT_SECRET_KEY_ENV]
            ?? throw new InvalidOperationException(AuthConstants.ErrorMessages.JWT_SECRET_KEY_NOT_CONFIGURED);

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