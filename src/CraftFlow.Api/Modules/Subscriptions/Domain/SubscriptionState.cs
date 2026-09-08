namespace CraftFlow.Api.Modules.Subscriptions.Domain;
public enum SubscriptionState
{
    Trial = 1,
    Active = 2,
    Suspended = 3,
    Expired = 4,
    Cancelled = 5
}

public enum SubscriptionTrigger
{
    Activate = 1,
    Suspend = 2,
    Expire = 3,
    Cancel = 4,
    Renew = 5
}
