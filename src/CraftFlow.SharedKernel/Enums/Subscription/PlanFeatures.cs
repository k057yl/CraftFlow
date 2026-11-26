namespace CraftFlow.SharedKernel.Enums.Subscription;

[Flags]
public enum PlanFeatures
{
    NONE = 0,
    PDF_EXPORT = 1 << 0,
    EXCEL_EXPORT = 1 << 1,
    ADVANCED_ANALYTICS = 1 << 2,
    MRP_PLANNING = 1 << 3
}