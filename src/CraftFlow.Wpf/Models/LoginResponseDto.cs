namespace CraftFlow.Wpf.Models;
public record LoginResponseDto(string Token, Guid TenantId, string FullName);
