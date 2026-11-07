namespace CraftFlow.Api.Core.Behaviors;

public interface IRequireQuotaValidation
{
    QuotaType QuotaType { get; }
}