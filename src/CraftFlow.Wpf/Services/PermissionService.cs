using CraftFlow.SharedKernel.Enums.Identity;

namespace CraftFlow.Wpf.Services;

public class PermissionService
{
    private static readonly Lazy<PermissionService> _instance = new(() => new PermissionService());
    public static PermissionService Instance => _instance.Value;

    public bool HasPermission(AppPermission permission)
    {
        var currentUser = ApiService.Instance.CurrentUser;
        if (currentUser == null) return false;

        return currentUser.Role.HasPermission(permission);
    }

    public bool IsSuperAdmin => ApiService.Instance.CurrentUser?.Role == TenantRole.SUPER_ADMIN;
    public bool IsOwner => ApiService.Instance.CurrentUser?.Role == TenantRole.OWNER;
}