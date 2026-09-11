namespace CraftFlow.Api.Modules.Identity.GetSubscriptionPayments;

public record PaymentDto(
    Guid Id,
    DateTime CreatedAtUtc,
    string PlanCode,
    int DaysAdded,
    string Description
);