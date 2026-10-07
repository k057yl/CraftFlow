namespace CraftFlow.Wpf.Services.Startup;
public class StartupResult
{
    public StartupResultType ResultType { get; set; }
    public object? InitialData { get; set; }
    public string? ErrorMessage { get; set; }
}