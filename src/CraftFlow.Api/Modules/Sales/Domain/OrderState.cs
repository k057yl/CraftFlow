namespace CraftFlow.Api.Modules.Sales.Domain
{
    public enum OrderState
    {
        Draft = 1,
        Confirmed = 2,
        Shipped = 3,
        Cancelled = 4
    }

    public enum OrderTrigger
    {
        Confirm = 1,
        Ship = 2,
        Cancel = 3
    }
}
