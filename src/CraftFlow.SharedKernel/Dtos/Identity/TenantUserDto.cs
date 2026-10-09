using CraftFlow.SharedKernel.Enums.Identity;

namespace CraftFlow.SharedKernel.Dtos.Identity;

public sealed record TenantUserDto(
    Guid Id,
    string FullName,
    string Email,
    TenantRole Role,
    bool IsActive
);