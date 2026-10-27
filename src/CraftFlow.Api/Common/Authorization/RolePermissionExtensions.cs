using CraftFlow.SharedKernel.Enums.Identity;

namespace CraftFlow.Api.Common.Authorization;
public static class RolePermissionExtensions
{
    private static readonly Dictionary<TenantRole, AppPermission> _rolePermissions = new()
    {
        // 1. Супер админ
        [TenantRole.SUPER_ADMIN] = AppPermission.ALL,

        // 2. Owner - Всё в рамках своей организации
        [TenantRole.OWNER] = AppPermission.ALL
            & ~AppPermission.SYSTEM_ADMIN
            & ~AppPermission.SUBSCRIPTION_MANAGE,

        // 3. Бухгалтер - Дашборды, приходы, списания, создание поставщиков/клиентов, продажи
        [TenantRole.ACCOUNTANT] =
            AppPermission.DASHBOARD_VIEW |
            AppPermission.FINANCE_VIEW |
            AppPermission.PROCUREMENT_VIEW | AppPermission.PROCUREMENT_RECEIVE | AppPermission.SUPPLIERS_MANAGE |
            AppPermission.SALES_VIEW | AppPermission.SALES_MANAGE | AppPermission.CUSTOMERS_MANAGE |
            AppPermission.INVENTORY_VIEW | AppPermission.INVENTORY_WRITEOFF,

        // 4. Технолог - Сырье, рецепты, запуск варки, списание, полные права на санитарию (мойка, график, приемка)
        [TenantRole.TECHNOLOGIST] =
            AppPermission.RAW_MATERIALS_MANAGE |
            AppPermission.RECIPES_MANAGE |
            AppPermission.PRODUCTION_VIEW | AppPermission.PRODUCTION_EXECUTE |
            AppPermission.INVENTORY_VIEW | AppPermission.INVENTORY_WRITEOFF |
            AppPermission.PROCUREMENT_VIEW |
            AppPermission.SANITATION_VIEW | AppPermission.SANITATION_SCHEDULE | AppPermission.SANITATION_EXECUTE | AppPermission.SANITATION_VERIFY,

        // 5. Менеджер (Sales Manager) - Только финансы, дашборды и остатки на складах
        [TenantRole.SALES_MANAGER] =
            AppPermission.DASHBOARD_VIEW |
            AppPermission.FINANCE_VIEW |
            AppPermission.SALES_VIEW | AppPermission.SALES_MANAGE |
            AppPermission.INVENTORY_VIEW,

        // 6. Опереатор / Воркер - Выполнение санитарии (мойка) и просмотр процессов
        [TenantRole.OPERATOR] =
            AppPermission.PRODUCTION_VIEW |
            AppPermission.SANITATION_VIEW | AppPermission.SANITATION_EXECUTE,

        // 7. Кладовщик (Storekeeper) - Приход, списание, управление складами, остатки
        [TenantRole.STOREKEEPER] =
            AppPermission.PROCUREMENT_VIEW | AppPermission.PROCUREMENT_RECEIVE |
            AppPermission.INVENTORY_VIEW | AppPermission.INVENTORY_WRITEOFF | AppPermission.WAREHOUSES_MANAGE |
            AppPermission.SANITATION_VIEW | AppPermission.SANITATION_EXECUTE,

        // 8. Аудитор - Только просмотр всего без права изменения
        [TenantRole.AUDITOR] =
            AppPermission.DASHBOARD_VIEW | AppPermission.FINANCE_VIEW |
            AppPermission.PROCUREMENT_VIEW | AppPermission.INVENTORY_VIEW |
            AppPermission.PRODUCTION_VIEW | AppPermission.SALES_VIEW | AppPermission.SANITATION_VIEW
    };

    public static bool HasPermission(this TenantRole role, AppPermission permission)
    {
        if (_rolePermissions.TryGetValue(role, out var permissions))
        {
            return (permissions & permission) == permission;
        }
        return false;
    }
}
