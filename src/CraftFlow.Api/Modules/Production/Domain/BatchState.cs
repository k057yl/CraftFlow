namespace CraftFlow.Api.Modules.Production.Domain;

public enum BatchState
{
    Draft = 1,
    InProgress = 2,
    ReadyForAging = 3,
    InAging = 4,
    Completed = 5,
    Discarded = 6
}

public enum BatchTrigger
{
    Start,
    Complete,
    TransferToAging,
    ReleaseFromAging,
    Discard
}