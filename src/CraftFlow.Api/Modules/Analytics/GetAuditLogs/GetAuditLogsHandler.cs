using CraftFlow.Api.Common.Persistence;
using CraftFlow.SharedKernel.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Analytics.GetAuditLogs;
public class GetAuditLogsHandler : IRequestHandler<GetAuditLogsQuery, Result<List<AuditLogDto>>>
{
    private readonly AppDbContext _dbContext;

    public GetAuditLogsHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<AuditLogDto>>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var logs = await _dbContext.AuditLogs
            .AsNoTracking()
            .OrderByDescending(a => a.CreatedAtUtc)
            .Take(50)
            .Select(a => new AuditLogDto(a.Id, a.EntityName, a.Action, a.Details, a.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return Result.Success(logs);
    }
}
