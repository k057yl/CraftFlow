namespace CraftFlow.SharedKernel.Security;

[AttributeUsage(AttributeTargets.Property)]
public sealed class SkipSanitizationAttribute : Attribute
{
}