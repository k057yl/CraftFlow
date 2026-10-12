namespace CraftFlow.Wpf.Services.Converters;

public static class UnitFormatter
{
    public static string Format(decimal amount, string? uomCode)
    {
        return uomCode?.ToLowerInvariant() switch
        {
            "g" when amount >= 1000m => $"{amount / 1000m:0.##} kg",
            "g" => $"{amount:0.##} g",
            "ml" when amount >= 1000m => $"{amount / 1000m:0.##} l",
            "ml" => $"{amount:0.##} ml",
            _ => $"{amount:0.##} {uomCode}"
        };
    }
}