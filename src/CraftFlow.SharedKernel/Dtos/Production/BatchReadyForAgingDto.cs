namespace CraftFlow.SharedKernel.Dtos.Production;

public record BatchReadyForAgingDto(
    Guid Id,
    string Name,
    int DefaultAgingDays,
    int UnitsCount = 1
);