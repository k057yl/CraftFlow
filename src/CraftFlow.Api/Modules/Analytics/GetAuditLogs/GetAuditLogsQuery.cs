using CraftFlow.SharedKernel.Dtos.Analytics;
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
) : IRequest<Result<List<AuditLogDto>>>;