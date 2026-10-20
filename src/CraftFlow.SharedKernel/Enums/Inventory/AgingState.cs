namespace CraftFlow.SharedKernel.Enums.Inventory;

public enum AgingState
{
    IN_CHAMBER = 1,
    READY_FOR_RELEASE = 2,
    RELEASED = 3,
    DISCARDED = 4
}

public enum AgingTrigger
{
    MARK_READY_FOR_RELEASE,
    RELEASE,
    DISCARD
}