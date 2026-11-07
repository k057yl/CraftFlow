namespace CraftFlow.SharedKernel.Enums;

[Flags]
public enum AppModule
{
    NONE = 0,
    CHEESE_MAKING = 1 << 0,
    BREWERY = 1 << 1,
    MEAT_PROCESSING = 1 << 2,
    FERMENTATION = 1 << 3,
    ADVANCED_ANALYTICS = 1 << 4,

    ALL = CHEESE_MAKING | BREWERY | MEAT_PROCESSING | FERMENTATION | ADVANCED_ANALYTICS
}