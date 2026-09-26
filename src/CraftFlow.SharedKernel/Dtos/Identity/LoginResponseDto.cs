using CraftFlow.Api.Modules.Identity.Domain;

namespace CraftFlow.Api.Modules.Identity.LoginUser;

public record LoginResponseDto(
    string Token,
    Guid TenantId,
    string FullName,
    string Email,
    TenantRole Role
);