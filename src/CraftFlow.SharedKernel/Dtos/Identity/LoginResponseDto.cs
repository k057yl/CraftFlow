using CraftFlow.SharedKernel.Enums.Identity;

namespace CraftFlow.SharedKernel.Dtos.Identity;

public record LoginResponseDto(
    string Token,
    Guid TenantId,
    string FullName,
    string Email,
    TenantRole Role
);