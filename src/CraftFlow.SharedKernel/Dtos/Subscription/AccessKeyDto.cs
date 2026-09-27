using CraftFlow.Api.Modules.Subscriptions.Domain;

namespace CraftFlow.SharedKernel.Dtos.Subscription;

public record AccessKeyDto(
    Guid Id,
    string Name,
    string MaskedKey,
    KeyStatus Status,
    DateTime CreatedAtUtc,
    DateTime? LastUsedAtUtc);