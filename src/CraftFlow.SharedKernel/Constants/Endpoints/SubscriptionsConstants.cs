namespace CraftFlow.SharedKernel.Constants.Endpoints;

public static class SubscriptionsConstants
{
    public const string REQUEST_INVOICE = "api/subscriptions/request-invoice";
    public const string SUBSCRIPTION_TEST_CONFIRM = "api/subscriptions/confirm";
    public const string PENDING_INVOICES = "api/subscriptions/invoices/pending";

    public static string ApproveInvoice(Guid id) => $"api/subscriptions/invoices/{id}/approve";
}