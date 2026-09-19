using CraftFlow.Api.Modules.Identity.Domain;

namespace CraftFlow.SharedKernel.Dtos.Auth;
public sealed record TenantUserDto(
    Guid Id,
    string FullName,
    string Email,
    TenantRole Role,
    bool IsActive
);