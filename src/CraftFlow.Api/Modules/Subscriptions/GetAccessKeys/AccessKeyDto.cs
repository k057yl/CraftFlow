using CraftFlow.Api.Modules.Subscriptions.Domain;

namespace CraftFlow.Api.Modules.Subscriptions.GetAccessKeys;

public record AccessKeyDto(
    Guid Id,
    string Name,
    string MaskedKey,
    KeyStatus Status,
    DateTime CreatedAtUtc,
    DateTime? LastUsedAtUtc);