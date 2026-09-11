namespace CraftFlow.Api.Modules.Identity.ManageSubscription;

public record ManageSubscriptionRequest(
    string PlanCode,
    int AddDays,
    bool IsActive
);