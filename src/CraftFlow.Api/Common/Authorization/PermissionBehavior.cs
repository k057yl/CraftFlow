using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;
using System.Security.Claims;

namespace CraftFlow.Api.Common.Authorization;

public class PermissionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PermissionBehavior(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not IRequirePermission permissionRequest)
        {
            return await next();
        }

        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null)
        {
            return (TResponse)Result.Failure(Error.Unauthorized(ErrorCodes.Auth.UNAUTHORIZED));
        }

        var userRole = GetUserRoleFromClaims(user);

        if (!userRole.HasPermission(permissionRequest.RequiredPermission))
        {
            return (TResponse)Result.Failure(Error.Forbidden(ErrorCodes.Auth.ACCESS_DENIED));
        }

        return await next();
    }

    private static TenantRole GetUserRoleFromClaims(ClaimsPrincipal user)
    {
        var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value
                        ?? user.FindFirst("role")?.Value
                        ?? user.FindFirst(AuthConstants.Claims.ROLE_SHORT)?.Value;

        if (Enum.TryParse<TenantRole>(roleClaim, ignoreCase: true, out var parsedRole)) return parsedRole;
        if (int.TryParse(roleClaim, out var roleInt)) return (TenantRole)roleInt;

        return TenantRole.NONE;
    }
}