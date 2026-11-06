namespace CraftFlow.SharedKernel.Enums.Saas;

[Flags]
public enum AppModule
{
    NONE = 0,
    CHEESE_MAKING = 1 << 0,
    BREWERY = 1 << 1,
    MEAT_FERMENTATION = 1 << 2,
    ADVANCED_ANALYTICS = 1 << 3,

    ALL = CHEESE_MAKING | BREWERY | MEAT_FERMENTATION | ADVANCED_ANALYTICS
}