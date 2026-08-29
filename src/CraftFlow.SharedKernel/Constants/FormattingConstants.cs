namespace CraftFlow.SharedKernel.Constants;

public static class FormattingConstants
{
    public const string BATCH_NUMBER_FORMAT = "LOT-{0:yyyyMMdd}-{1}";
    public const string CURRENCY_FORMAT = "${0:F2}";
    public const string DISPLAY_INFO_REQUIREMENT_FORMAT = "{0}: Нужно {1:F2} | Доступно {2:F2} {3}";
    public const string CHECKMARK_SUFFICIENT = "✔";
    public const string CHECKMARK_INSUFFICIENT = "❌ (НЕ ХВАТАЕТ!)";

    public const string ICON_RAW_MATERIAL = "🥛";
    public const string ICON_PRODUCTION_BATCH = "🧀";
    public const string ICON_AGING_LOT = "⏳";

    public const string TRACE_BATCH_TITLE_FORMAT = "Варка #{0}";
    public const string TRACE_BATCH_DETAILS_FORMAT = "Статус: {0} | Запущена: {1:dd.MM.yyyy}";
    public const string TRACE_AGING_TITLE_FORMAT = "Камера: {0}";
    public const string TRACE_AGING_DETAILS_FORMAT = "Партия: {0} ({1})";
    public const string TRACE_RAW_DETAILS_FORMAT = "[{0}]";
}