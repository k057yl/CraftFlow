using CraftFlow.SharedKernel.Enums;

namespace CraftFlow.SharedKernel.Dtos.Subscription;

public record ModuleSubscriptionStatusDto(
    AppModule Module,
    bool IsConnected,
    string TierSuffix,
    string PlanName,
    DateTime? ExpiresAtUtc
);