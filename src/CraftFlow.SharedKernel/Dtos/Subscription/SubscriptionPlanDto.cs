namespace CraftFlow.SharedKernel.Dtos.Subscription;

public record SubscriptionPlanDto(
    Guid Id,
    string Code,
    string Name
);