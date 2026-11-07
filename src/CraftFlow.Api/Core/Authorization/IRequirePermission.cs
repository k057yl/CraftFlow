using CraftFlow.SharedKernel.Enums.Identity;

namespace CraftFlow.Api.Core.Authorization;

public interface IRequirePermission
{
    AppPermission RequiredPermission { get; }
}