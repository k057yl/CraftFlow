namespace CraftFlow.Api.Modules.Identity.Subscriptions.ManageSubscription;

public record ManageSubscriptionRequest(
    string PlanCode,
    int AddDays,
    bool IsActive
);