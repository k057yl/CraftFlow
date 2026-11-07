namespace CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.ManageSubscription;

public record ManageSubscriptionRequest(
    string PlanCode,
    int AddDays,
    bool IsActive
);