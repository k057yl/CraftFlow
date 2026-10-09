using CraftFlow.SharedKernel.Enums.Identity;

namespace CraftFlow.SharedKernel.Dtos.Identity;

public record CreateTenantUserRequest(
    string Email,
    string Password,
    string FullName,
    TenantRole Role
);