using CraftFlow.SharedKernel.Enums.Identity;

namespace CraftFlow.Wpf.Services;

public static class RolePermissionExtensions
{
    public static bool HasPermission(this TenantRole role, AppPermission permission)
    {
        var permissions = GetPermissionsForRole(role);
        return (permissions & permission) == permission;
    }

    public static AppPermission GetPermissionsForRole(TenantRole role)
    {
        return role switch
        {
            TenantRole.SUPER_ADMIN => AppPermission.ALL,

            TenantRole.OWNER =>
                AppPermission.DASHBOARD_VIEW |
                AppPermission.RECIPES_MANAGE |
                AppPermission.RAW_MATERIALS_MANAGE |
                AppPermission.PROCUREMENT_VIEW |
                AppPermission.PROCUREMENT_RECEIVE |
                AppPermission.SUPPLIERS_MANAGE |
                AppPermission.PRODUCTION_VIEW |
                AppPermission.PRODUCTION_EXECUTE |
                AppPermission.SALES_VIEW |
                AppPermission.SALES_MANAGE |
                AppPermission.CUSTOMERS_MANAGE |
                AppPermission.INVENTORY_WRITEOFF |
                AppPermission.WAREHOUSES_MANAGE |
                AppPermission.SANITATION_VIEW |
                AppPermission.SANITATION_SCHEDULE |
                AppPermission.SANITATION_EXECUTE |
                AppPermission.SANITATION_VERIFY |
                AppPermission.USERS_MANAGE |
                AppPermission.SETTINGS_MANAGE,

            TenantRole.TECHNOLOGIST =>
                AppPermission.DASHBOARD_VIEW |
                AppPermission.RECIPES_MANAGE |
                AppPermission.RAW_MATERIALS_MANAGE |
                AppPermission.PRODUCTION_VIEW |
                AppPermission.PRODUCTION_EXECUTE |
                AppPermission.INVENTORY_WRITEOFF |
                AppPermission.SANITATION_VIEW |
                AppPermission.SANITATION_SCHEDULE |
                AppPermission.SANITATION_EXECUTE |
                AppPermission.SANITATION_VERIFY,

            TenantRole.STOREKEEPER =>
                AppPermission.DASHBOARD_VIEW |
                AppPermission.PROCUREMENT_VIEW |
                AppPermission.PROCUREMENT_RECEIVE |
                AppPermission.INVENTORY_WRITEOFF |
                AppPermission.WAREHOUSES_MANAGE |
                AppPermission.SANITATION_VIEW |
                AppPermission.SANITATION_EXECUTE,

            TenantRole.SALES_MANAGER =>
                AppPermission.DASHBOARD_VIEW |
                AppPermission.SALES_VIEW |
                AppPermission.SALES_MANAGE |
                AppPermission.CUSTOMERS_MANAGE,

            TenantRole.ACCOUNTANT =>
                AppPermission.DASHBOARD_VIEW |
                AppPermission.PROCUREMENT_VIEW |
                AppPermission.SALES_VIEW |
                AppPermission.INVENTORY_WRITEOFF |
                AppPermission.RECIPES_MANAGE,

            _ => AppPermission.NONE
        };
    }
}