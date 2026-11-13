namespace CraftFlow.SharedKernel.Constants.Endpoints;

public static class FermentationConstants
{
    public const string BASE = "/api/fermentation";
    public const string BATCHES = $"{BASE}/batches";
    public const string READINGS = "readings";
    public const string MOVE_TO_COLD = "move-to-cold";
    public const string COMPLETE = "complete";
    public const string DISCARD = "discard";
}