using CraftFlow.Api.Modules.Identity.Domain;

namespace CraftFlow.SharedKernel.Dtos.Auth;

public record CreateTenantUserRequest(
    string Email,
    string Password,
    string FullName,
    TenantRole Role
);