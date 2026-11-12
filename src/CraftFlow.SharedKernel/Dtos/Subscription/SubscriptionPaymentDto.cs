namespace CraftFlow.SharedKernel.Dtos.Subscription;

public record SubscriptionPaymentDto(
    Guid Id,
    string PlanCode,
    int DaysAdded,
    string Description,
    DateTime CreatedAtUtc
);