using CraftFlow.Api.Modules.CoreDomainServices.Identity.Domain;
using System.Security.Claims;


namespace CraftFlow.Api.Core.Infrastructure.Identity;

public interface ITokenService
{
    string GenerateJwtToken(User user, OrganizationMember? member = null, bool isSystemAdmin = false);
    string GenerateRefreshToken();
    string HashRefreshToken(string refreshToken);
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}