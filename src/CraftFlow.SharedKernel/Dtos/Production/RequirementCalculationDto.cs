using CraftFlow.SharedKernel.Constants;

namespace CraftFlow.SharedKernel.Dtos.Production;

public record RequirementCalculationDto(
    string MaterialName,
    decimal RequiredQty,
    decimal AvailableQty,
    bool IsSufficient
)
{
    public string DisplayInfo => string.Format(
        FormattingConstants.DISPLAY_INFO_REQUIREMENT_FORMAT,
        MaterialName,
        Math.Round(RequiredQty, 3),
        Math.Round(AvailableQty, 3),
        IsSufficient ? FormattingConstants.CHECKMARK_SUFFICIENT : FormattingConstants.CHECKMARK_INSUFFICIENT
    );
}