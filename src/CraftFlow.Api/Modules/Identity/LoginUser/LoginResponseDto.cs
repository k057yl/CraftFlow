namespace CraftFlow.Api.Modules.Identity.LoginUser;

public record LoginResponseDto(
    string Token,
    Guid TenantId,
    string FullName,
    string Email,
    bool IsAdmin
);