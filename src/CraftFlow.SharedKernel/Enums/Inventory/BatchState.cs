namespace CraftFlow.SharedKernel.Enums.Inventory;

public enum BatchState
{
    DRAFT = 1,
    IN_PROGRESS = 2,
    READY_FOR_AGING = 3,
    IN_AGING = 4,
    COMPLETED = 5,
    DISCARDED = 6
}

public enum BatchTrigger
{
    START,
    COMPLETE,
    TRANSFER_TO_AGING,
    RELEASE_FROM_AGING,
    DISCARD
}