namespace CraftFlow.Wpf.Services.Scales;

public interface IScaleService
{
    event EventHandler<decimal>? WeightChanged;
    bool IsConnected { get; }
    Task ConnectAsync(string portName);
    Task DisconnectAsync();
}