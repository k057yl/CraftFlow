namespace CraftFlow.Api.Modules.Production.Domain;
public enum BatchStatus
{
    Draft = 1,
    InProgress = 2,
    Completed = 3,
    TransferredToAging = 4,
    Cancelled = 5
}