using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.Wpf.Services;
using System.Windows;
using System.Windows.Markup;

namespace CraftFlow.Wpf.MarkupExtensions;

public class HasPermissionExtension : MarkupExtension
{
    public AppPermission Permission { get; set; }

    public HasPermissionExtension() { }

    public HasPermissionExtension(AppPermission permission)
    {
        Permission = permission;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        bool hasAccess = PermissionService.Instance.HasPermission(Permission);
        return hasAccess ? Visibility.Visible : Visibility.Collapsed;
    }
}