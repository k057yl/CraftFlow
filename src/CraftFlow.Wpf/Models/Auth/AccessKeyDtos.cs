namespace CraftFlow.Wpf.Models.Auth;

public record GenerateKeyResponseDto(
    string RawKey,
    string KeyName,
    string KeyPrefix,
    string KeySuffix);

public record AccessKeyDto(
    Guid Id,
    string Name,
    string MaskedKey,
    int Status,
    DateTime CreatedAtUtc,
    DateTime? LastUsedAtUtc);