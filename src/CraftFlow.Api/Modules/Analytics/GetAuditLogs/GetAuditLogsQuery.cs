using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Analytics.GetAuditLogs;
public record GetAuditLogsQuery : IRequest<Result<List<AuditLogDto>>>;