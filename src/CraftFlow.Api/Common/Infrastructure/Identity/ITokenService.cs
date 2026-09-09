using CraftFlow.Api.Modules.Identity.Domain;
using System.Security.Claims;

namespace CraftFlow.Api.Common.Infrastructure.Identity;

public interface ITokenService
{
    string GenerateJwtToken(User user);
    string GenerateRefreshToken();
    string HashRefreshToken(string refreshToken);
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}