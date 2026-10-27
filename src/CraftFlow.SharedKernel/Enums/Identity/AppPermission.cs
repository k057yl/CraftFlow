namespace CraftFlow.SharedKernel.Enums.Identity;

[Flags]
public enum AppPermission : long
{
    NONE = 0,

    // --- Система и Подписка (Только SuperAdmin) ---
    SYSTEM_ADMIN = 1L << 0,
    SUBSCRIPTION_MANAGE = 1L << 1,

    // --- Организация и Пользователи (Owner / SuperAdmin) ---
    USERS_MANAGE = 1L << 2,
    SETTINGS_MANAGE = 1L << 3,

    // --- Дашборд и Аналитика ---
    DASHBOARD_VIEW = 1L << 4,
    FINANCE_VIEW = 1L << 5,

    // --- Закупки и Поставщики ---
    PROCUREMENT_VIEW = 1L << 6,
    PROCUREMENT_RECEIVE = 1L << 7,  // Приход
    SUPPLIERS_MANAGE = 1L << 8,     // Создание/редактирование поставщиков

    // --- Продажи и Клиенты ---
    SALES_VIEW = 1L << 9,
    SALES_MANAGE = 1L << 10,
    CUSTOMERS_MANAGE = 1L << 11,    // Создание/редактирование клиентов

    // --- Склад и Сырье ---
    INVENTORY_VIEW = 1L << 12,      // Остатки
    INVENTORY_WRITEOFF = 1L << 13,  // Списание
    RAW_MATERIALS_MANAGE = 1L << 14,// Добавление нового сырья
    WAREHOUSES_MANAGE = 1L << 15,

    // --- Производство и Рецепты ---
    PRODUCTION_VIEW = 1L << 16,
    PRODUCTION_EXECUTE = 1L << 17, // Запуск варки
    RECIPES_MANAGE = 1L << 18,     // Рецепты

    // --- Санитария и Обслуживание ---
    SANITATION_VIEW = 1L << 19,
    SANITATION_SCHEDULE = 1L << 20, // График моек
    SANITATION_EXECUTE = 1L << 21,  // Мойка (отметка о выполнении)
    SANITATION_VERIFY = 1L << 22,   // Приемка работ (технолог)

    // Все доступные права
    ALL = ~NONE
}