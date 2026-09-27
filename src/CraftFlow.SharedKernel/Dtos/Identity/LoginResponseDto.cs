using CraftFlow.Api.Modules.Identity.Domain;

namespace CraftFlow.SharedKernel.Dtos.Identity;

public record LoginResponseDto(
    string Token,
    Guid TenantId,
    string FullName,
    string Email,
    TenantRole Role
);