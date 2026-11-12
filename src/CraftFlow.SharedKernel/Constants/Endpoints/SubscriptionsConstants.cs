namespace CraftFlow.SharedKernel.Constants.Endpoints;

public static class SubscriptionsConstants
{
    public const string REQUEST_INVOICE = "api/subscriptions/request-invoice";
    public const string ACTIVATE_FREE_MODULE = "api/subscriptions/activate-free";
    public const string SUBSCRIPTION_TEST_CONFIRM = "api/subscriptions/confirm";
    public const string PENDING_INVOICES = "api/subscriptions/invoices/pending";
    public const string MY_SUBSCRIPTIONS = "api/subscriptions/my";
    public const string PLANS = "api/subscriptions/plans";

    public static string ApproveInvoice(Guid id) => $"api/subscriptions/invoices/{id}/approve";
}