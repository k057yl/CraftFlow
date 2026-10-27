using CraftFlow.Api.Common.Authorization;
using CraftFlow.SharedKernel.Dtos.Analytics;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Analytics.GetAuditLogs;

public record GetAuditLogsQuery(
    string? SearchTerm,
    string? EntityName,
    string? Action,
    Guid? UserId,
    DateTime? FromDate,
    DateTime? ToDate
) : IRequest<Result<List<AuditLogDto>>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.SETTINGS_MANAGE;
}