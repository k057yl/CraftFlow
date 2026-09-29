namespace CraftFlow.SharedKernel.Enums;

public enum AgingState
{
    InChamber = 1,
    ReadyForRelease = 2,
    Released = 3,
    Discarded = 4
}

public enum AgingTrigger
{
    MarkReadyForRelease,
    Release,
    Discard
}