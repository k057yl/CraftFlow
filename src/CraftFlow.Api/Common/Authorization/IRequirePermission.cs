using CraftFlow.SharedKernel.Enums.Identity;

namespace CraftFlow.Api.Common.Authorization;

public interface IRequirePermission
{
    AppPermission RequiredPermission { get; }
}