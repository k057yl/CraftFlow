namespace CraftFlow.Wpf.Models;

public record LookupItem(Guid Id, string Name, string? Code = null)
{
    public bool IsArchived { get; init; } = false;
}